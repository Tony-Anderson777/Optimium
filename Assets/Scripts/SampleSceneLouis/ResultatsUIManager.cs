using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Optimisation.Models;


public class ResultatsUIManager : MonoBehaviour
{
    private ScrollView _resultatsScroll;
    private UIDocument _document;

    void Awake()
    {
        Debug.Log("ResultatsUIManager: Awake appelé");
        
        _document = GetComponent<UIDocument>();
        if (_document == null)
        {
            Debug.LogError("ResultatsUIManager: UIDocument non trouvé sur ce GameObject!");
            return;
        }

        // Charger automatiquement le fichier UXML
        var visualTree = Resources.Load<VisualTreeAsset>("OptimisationResultats");
        if (visualTree != null)
        {
            _document.visualTreeAsset = visualTree;
            Debug.Log("ResultatsUIManager: Fichier UXML chargé automatiquement");
        }
        else
        {
            Debug.LogError("ResultatsUIManager: Fichier UXML OptimisationResultats non trouvé dans Resources!");
        }

        // Attendre que le document soit initialisé
        StartCoroutine(InitialiserUI());
    }

    private System.Collections.IEnumerator InitialiserUI()
    {
        // Attendre que le document soit prêt
        yield return new WaitForEndOfFrame();
        
        Debug.Log("ResultatsUIManager: Initialisation de l'UI");
        
        var root = _document.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("ResultatsUIManager: rootVisualElement est null!");
            yield break;
        }

        _resultatsScroll = root.Q<ScrollView>("ResultatsScroll");
        if (_resultatsScroll == null)
        {
            Debug.LogError("ResultatsUIManager: ScrollView 'ResultatsScroll' non trouvé!");
            yield break;
        }
        
        Debug.Log("ResultatsUIManager: ScrollView trouvé avec succès");

        // Vérifier les résultats
        if (ResultatsHolder.resultats == null)
        {
            Debug.LogWarning("ResultatsUIManager: ResultatsHolder.resultats est null!");
            AfficherMessage("Aucun résultat disponible");
        }
        else
        {
            Debug.Log($"ResultatsUIManager: {ResultatsHolder.resultats.Count} résultats trouvés");
            AfficherResultats(ResultatsHolder.resultats);
        }

        var exportButton = root.Q<Button>("ExportExcelButton");
        if (exportButton != null)
        {
            Debug.Log("ResultatsUIManager: Bouton export trouvé");
            exportButton.clicked += () => ExcelExporter.ExporterResultats(ResultatsHolder.resultats);
        }
        else
        {
            Debug.LogWarning("ResultatsUIManager: Bouton 'ExportExcelButton' non trouvé!");
        }
    }

    public void AfficherResultats(List<ResultatOptimisation> resultats)
    {
        if (_resultatsScroll == null)
        {
            Debug.LogError("ResultatsUIManager: _resultatsScroll est null dans AfficherResultats!");
            return;
        }

        _resultatsScroll.Clear();
        
        if (resultats == null || resultats.Count == 0)
        {
            AfficherMessage("Aucun résultat d'optimisation disponible");
            return;
        }

        foreach (var res in resultats)
        {
            string texte = $"Réservation {res.Reservation.CodeAnalytique} → {res.SalleOptimisee} (Taux: {res.TauxOccupation:P0})";
            if (!string.IsNullOrEmpty(res.RaisonNonAttribution))
                texte += $" | {res.RaisonNonAttribution}";
            
            var label = new Label(texte);
            label.style.marginBottom = 5;
            label.style.color = Color.white;
            _resultatsScroll.Add(label);
            
            Debug.Log($"ResultatsUIManager: Ajouté - {texte}");
        }
    }

    private void AfficherMessage(string message)
    {
        if (_resultatsScroll != null)
        {
            var label = new Label(message);
            label.style.color = Color.yellow;
            _resultatsScroll.Add(label);
        }
    }
}

public static class ResultatsHolder
{
    public static List<ResultatOptimisation> resultats;
} 