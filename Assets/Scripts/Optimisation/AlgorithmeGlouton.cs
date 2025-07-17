using System;
using System.Collections.Generic;
using System.Linq;
using Optimisation.Models;

public static class AlgorithmeGlouton
{
    /// <summary>
    /// Optimise l'affectation des réservations aux salles selon l'algorithme glouton.
    /// </summary>
    /// <param name="reservations">Liste des réservations à traiter</param>
    /// <param name="salles">Liste des salles disponibles (SOroom)</param>
    /// <param name="seuilOptimal">Taux d'occupation optimal (ex: 0.85)</param>
    /// <param name="seuilMinimal">Taux d'occupation minimal (ex: 0.3)</param>
    /// <param name="bufferMinutes">Buffer anti-conflit en minutes</param>
    /// <returns>Liste des résultats d'optimisation</returns>
    public static List<ResultatOptimisation> Optimiser(
        List<Reservation> reservations,
        List<SOroom> salles,
        float seuilOptimal,
        float seuilMinimal,
        int bufferMinutes = 0)
    {
        var resultats = new List<ResultatOptimisation>();
        var plannings = salles.ToDictionary(s => s.roomName, s => new List<(DateTime, DateTime)>());

        // Tri : créneaux les plus courts en premier, puis effectif décroissant
        var reservationsTriees = reservations
            .OrderBy(r => GetDureeMinutes(r))
            .ThenByDescending(r => r.NombreInscrits)
            .ToList();

        foreach (var reservation in reservationsTriees)
        {
            var resultat = new ResultatOptimisation
            {
                Reservation = reservation,
                SalleOptimisee = "Aucune salle adaptée",
                TauxOccupation = 0f,
                RaisonNonAttribution = ""
            };

            string meilleureSalle = null;
            float meilleurTaux = -1f;
            bool salleTrouvee = false;
            bool conflitHoraire = false;

            foreach (var salle in salles.OrderBy(s => s.capacity))
            {
                if (salle.capacity >= reservation.NombreInscrits && salle.isAvailable)
                {
                    salleTrouvee = true;
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

            if (meilleureSalle != null)
            {
                resultat.SalleOptimisee = meilleureSalle;
                resultat.TauxOccupation = meilleurTaux;
                var debut = DateTime.Parse($"{reservation.Date} {reservation.HeureDebut}");
                var fin = DateTime.Parse($"{reservation.Date} {reservation.HeureFin}");
                plannings[meilleureSalle].Add((debut, fin));
                plannings[meilleureSalle].Sort();
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

    /// <summary>
    /// Calcule la durée en minutes d'une réservation.
    /// </summary>
    private static int GetDureeMinutes(Reservation r)
    {
        var debut = DateTime.Parse($"{r.Date} {r.HeureDebut}");
        var fin = DateTime.Parse($"{r.Date} {r.HeureFin}");
        return (int)(fin - debut).TotalMinutes;
    }

    /// <summary>
    /// Vérifie si une salle est libre pour la réservation donnée, en tenant compte du buffer.
    /// </summary>
    private static bool SalleLibre(List<(DateTime, DateTime)> planning, Reservation reservation, int bufferMinutes)
    {
        var debut = DateTime.Parse($"{reservation.Date} {reservation.HeureDebut}");
        var fin = DateTime.Parse($"{reservation.Date} {reservation.HeureFin}");
        var debutBuffer = debut.AddMinutes(-bufferMinutes);
        var finBuffer = fin.AddMinutes(bufferMinutes);
        foreach (var creneau in planning)
        {
            if (!(finBuffer <= creneau.Item1 || debutBuffer >= creneau.Item2))
            {
                return false;
            }
        }
        return true;
    }
} 