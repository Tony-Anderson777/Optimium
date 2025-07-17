using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using Optimisation.Models;
using System;

public class OptimisationManager : MonoBehaviour
{
    [Header("Paramètres d'optimisation")]
    public float seuilOptimal = 0.85f; // 85%
    public float seuilMinimal = 0.3f;  // 30%
    public int bufferMinutes = 0;
    
    [Header("Références")]
    public SOdatabase databaseSalles;
    
    private List<Reservation> reservations;
    private List<ResultatOptimisation> resultatsOptimisation;
    
    public void LancerOptimisation()
    {
        Debug.Log("OptimisationManager: Début de l'optimisation");
        
        // Charger les réservations depuis le fichier Excel
        ChargerReservations();
        
        if (reservations == null || reservations.Count == 0)
        {
            Debug.LogError("OptimisationManager: Aucune réservation à traiter");
            return;
        }
        
        // Lancer l'algorithme glouton
        resultatsOptimisation = OptimiserGlouton();
        
        // Transférer les résultats
        ResultatsHolder.resultats = resultatsOptimisation;
        
        Debug.Log($"OptimisationManager: {resultatsOptimisation.Count} résultats générés");
        
        // Changer de scène
        SceneManager.LoadScene("OptimisationScene");
    }
    
    private void ChargerReservations()
    {
        // Pour l'instant, créer des données de test
        // TODO: Charger depuis le fichier Excel réel
        reservations = new List<Reservation>();
        
        // Données de test
        var testReservations = new[]
        {
            new Reservation { CodeAnalytique = "INFO1", NombreInscrits = 25, Date = "2024-01-15", HeureDebut = "09:00", HeureFin = "12:00", NomSalle = "A101" },
            new Reservation { CodeAnalytique = "INFO2", NombreInscrits = 30, Date = "2024-01-15", HeureDebut = "14:00", HeureFin = "17:00", NomSalle = "B202" },
            new Reservation { CodeAnalytique = "INFO3", NombreInscrits = 15, Date = "2024-01-15", HeureDebut = "09:00", HeureFin = "11:00", NomSalle = "C303" },
            new Reservation { CodeAnalytique = "INFO4", NombreInscrits = 40, Date = "2024-01-15", HeureDebut = "13:00", HeureFin = "16:00", NomSalle = "D404" },
        };
        
        reservations.AddRange(testReservations);
        Debug.Log($"OptimisationManager: {reservations.Count} réservations chargées");
    }
    
    private List<ResultatOptimisation> OptimiserGlouton()
    {
        var resultats = new List<ResultatOptimisation>();
        
        if (databaseSalles == null || databaseSalles.rooms == null)
        {
            Debug.LogError("OptimisationManager: Database des salles non disponible");
            return resultats;
        }
        
        // Trier les réservations par priorité (durée croissante, puis effectif décroissant)
        var reservationsTriees = reservations.OrderBy(r => GetDureeMinutes(r))
                                            .ThenByDescending(r => r.NombreInscrits)
                                            .ToList();
        
        // Planification par salle
        var plannings = new Dictionary<string, List<(DateTime debut, DateTime fin)>>();
        foreach (var salle in databaseSalles.rooms)
        {
            plannings[salle.roomName] = new List<(DateTime debut, DateTime fin)>();
        }
        
        foreach (var reservation in reservationsTriees)
        {
            var resultat = new ResultatOptimisation
            {
                Reservation = reservation,
                SalleOptimisee = "Aucune salle adaptée",
                TauxOccupation = 0f,
                RaisonNonAttribution = ""
            };
            
            // Trouver la meilleure salle
            string meilleureSalle = null;
            float meilleurTaux = -1f;
            bool salleTrouvee = false;
            bool conflitHoraire = false;
            
            foreach (var salle in databaseSalles.rooms.OrderBy(s => s.capacity))
            {
                if (salle.capacity >= reservation.NombreInscrits)
                {
                    salleTrouvee = true;
                    
                    // Vérifier si la salle est libre
                    if (SalleLibre(plannings[salle.roomName], reservation, bufferMinutes))
                    {
                        float taux = (float)reservation.NombreInscrits / salle.capacity;
                        if (taux > meilleurTaux)
                        {
                            meilleureSalle = salle.roomName;
                            meilleurTaux = taux;
                        }
                    }
                    else
                    {
                        conflitHoraire = true;
                    }
                }
            }
            
            // Assigner la salle si trouvée
            if (meilleureSalle != null)
            {
                resultat.SalleOptimisee = meilleureSalle;
                resultat.TauxOccupation = meilleurTaux;
                
                // Ajouter au planning
                var debut = DateTime.Parse($"{reservation.Date} {reservation.HeureDebut}");
                var fin = DateTime.Parse($"{reservation.Date} {reservation.HeureFin}");
                plannings[meilleureSalle].Add((debut, fin));
                plannings[meilleureSalle].Sort();
                
                // Déterminer la raison
                if (meilleurTaux >= seuilOptimal)
                    resultat.RaisonNonAttribution = $"Taux optimal ({meilleurTaux:P0})";
                else if (meilleurTaux <= seuilMinimal)
                    resultat.RaisonNonAttribution = $"Sous-utilisé ({meilleurTaux:P0})";
            }
            else
            {
                if (!salleTrouvee)
                    resultat.RaisonNonAttribution = "Capacité insuffisante";
                else if (conflitHoraire)
                    resultat.RaisonNonAttribution = "Conflit horaire";
                else
                    resultat.RaisonNonAttribution = "Erreur allocation";
            }
            
            resultats.Add(resultat);
        }
        
        return resultats;
    }
    
    private int GetDureeMinutes(Reservation reservation)
    {
        var debut = DateTime.Parse($"{reservation.Date} {reservation.HeureDebut}");
        var fin = DateTime.Parse($"{reservation.Date} {reservation.HeureFin}");
        return (int)(fin - debut).TotalMinutes;
    }
    
    private bool SalleLibre(List<(DateTime debut, DateTime fin)> planning, Reservation reservation, int bufferMinutes)
    {
        var debut = DateTime.Parse($"{reservation.Date} {reservation.HeureDebut}");
        var fin = DateTime.Parse($"{reservation.Date} {reservation.HeureFin}");
        
        var debutBuffer = debut.AddMinutes(-bufferMinutes);
        var finBuffer = fin.AddMinutes(bufferMinutes);
        
        foreach (var creneau in planning)
        {
            if (!(finBuffer <= creneau.debut || debutBuffer >= creneau.fin))
            {
                return false;
            }
        }
        
        return true;
    }
} 