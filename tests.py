import pytest
import pandas as pd
from app_optimisation_salles import is_overlapping, is_room_free, optimiser_reservations

# Tests unitaires
def test_is_overlapping():
    """Test la fonction is_overlapping."""
    # Cas où les créneaux se chevauchent
    assert is_overlapping(
        pd.Timestamp('2024-01-01 09:00'), pd.Timestamp('2024-01-01 10:00'),
        pd.Timestamp('2024-01-01 09:30'), pd.Timestamp('2024-01-01 10:30')
    )
    
    # Cas où les créneaux ne se chevauchent pas
    assert not is_overlapping(
        pd.Timestamp('2024-01-01 09:00'), pd.Timestamp('2024-01-01 10:00'),
        pd.Timestamp('2024-01-01 10:30'), pd.Timestamp('2024-01-01 11:30')
    )

def test_is_room_free():
    """Test la fonction is_room_free."""
    # Créneau existant
    schedule = [(pd.Timestamp('2024-01-01 09:00'), pd.Timestamp('2024-01-01 10:00'))]
    
    # Créneau qui chevauche
    assert not is_room_free(
        schedule,
        pd.Timestamp('2024-01-01 09:30'), pd.Timestamp('2024-01-01 10:30')
    )
    
    # Créneau qui ne chevauche pas
    assert is_room_free(
        schedule,
        pd.Timestamp('2024-01-01 10:30'), pd.Timestamp('2024-01-01 11:30')
    )
    
    # Créneau avec durée inférieure à la durée minimale
    assert not is_room_free(
        schedule,
        pd.Timestamp('2024-01-01 10:30'), pd.Timestamp('2024-01-01 10:31')
    )

def test_optimiser_reservations():
    """Test la fonction optimiser_reservations."""
    # Données de test
    df_salles = pd.DataFrame({
        COLONNE_NOM_SALLE: ['Salle A', 'Salle B', 'Salle C'],
        COLONNE_CAPACITE_SALLE: [50, 100, 150]
    })
    
    df_reservations = pd.DataFrame({
        COLONNE_NOMBRE_INSCRIT: [30, 80, 120],
        COLONNE_DATE_BOOKING: ['2024-01-01', '2024-01-01', '2024-01-01'],
        COLONNE_HEURE_DEBUT_BOOKING: ['09:00', '10:00', '11:00'],
        COLONNE_HEURE_FIN_BOOKING: ['10:00', '11:00', '12:00']
    })
    
    # Optimisation
    result = optimiser_reservations(df_reservations, df_salles)
    
    # Vérifications
    assert result is not None
    assert len(result) == 3
    assert result[COLONNE_SALLE_OPTIMISEE].notna().all()
    assert result['RatioOptimise'].notna().all()
    
    # Vérification des ratios
    ratios = result['RatioOptimise'].tolist()
    assert all(ratio >= 1.0 for ratio in ratios)
    assert all(ratio <= MAX_RATIO_OPTIMISATION for ratio in ratios)

if __name__ == "__main__":
    pytest.main(["-v", "-s", __file__])
