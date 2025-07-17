using System.Collections.Generic;
using Optimisation.Models;
using UnityEngine;

public static class ExcelExporter
{
    public static void ExporterResultats(List<ResultatOptimisation> resultats)
    {
        Debug.Log("ExcelExporter: Export des résultats");
        
        if (resultats == null || resultats.Count == 0)
        {
            Debug.LogWarning("ExcelExporter: Aucun résultat à exporter");
            return;
        }
        
        // Pour l'instant, juste afficher les résultats dans la console
        Debug.Log($"ExcelExporter: Export de {resultats.Count} résultats");
        
        foreach (var resultat in resultats)
        {
            Debug.Log($"Réservation {resultat.Reservation.CodeAnalytique} → {resultat.SalleOptimisee} (Taux: {resultat.TauxOccupation:P0})");
        }
        
        // TODO: Implémenter l'export Excel réel avec NPOI
        Debug.Log("ExcelExporter: Export terminé (simulation)");
    }
} 