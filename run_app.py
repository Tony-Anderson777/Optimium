# run_app.py
import streamlit.web.cli as stcli
import os
import sys

# Le nom de votre script principal
filename = "opti.py"

# Construit le chemin absolu vers le script
# Cela est nécessaire pour que PyInstaller trouve le script dans le bundle
dir_path = os.path.dirname(sys.argv[0])
script_path = os.path.join(dir_path, filename)

# Lance Streamlit
sys.argv = ["streamlit", "run", script_path, "--server.runOnSave", "false"]
sys.exit(stcli.main())
