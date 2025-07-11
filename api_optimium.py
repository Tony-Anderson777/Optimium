# api_optimisation.py
from fastapi import FastAPI, File, UploadFile, Form
from fastapi.responses import StreamingResponse
import pandas as pd
import os
from app_optimisation_salles import optimiser_glouton, charger_salles, SEUIL_BON_DEFAULT, SEUIL_BAS_DEFAULT, BUFFER_DEFAULT, NOM_FICHIER_SALLES, exporter_excel
from io import BytesIO

app = FastAPI()

@app.post("/optimiser_excel")
async def optimiser_excel(
    fichier_resa: UploadFile = File(...),
    seuil_bon: float = Form(SEUIL_BON_DEFAULT),
    seuil_bas: float = Form(SEUIL_BAS_DEFAULT),
    buffer_minutes: int = Form(BUFFER_DEFAULT),
    algo: str = Form("glouton")  # <--- Ajout du choix d'algo
):
    df_resa = pd.read_excel(fichier_resa.file)
    path_salles = os.path.join(os.path.dirname(__file__), NOM_FICHIER_SALLES)
    df_salles = charger_salles(path_salles)
    # Choix de l'algo
    if algo == "genetique":
        from app_optimisation_salles import optimiser_genetique
        # Tu peux aussi ajouter des paramètres pour l'algo génétique si besoin
        df_optimise = optimiser_genetique(df_resa, df_salles, seuil_bon, seuil_bas)
    else:
        df_optimise = optimiser_glouton(df_resa, df_salles, seuil_bon, seuil_bas, buffer_minutes)
    buffer = exporter_excel(df_optimise, seuil_bon, seuil_bas)
    buffer.seek(0)
    return StreamingResponse(
        buffer,
        media_type="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        headers={"Content-Disposition": "attachment; filename=reservations_optimisees.xlsx"}
    )
