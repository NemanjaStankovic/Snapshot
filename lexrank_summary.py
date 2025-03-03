import sys
from lexrank import LexRank
from lexrank import STOPWORDS

def debug_print(message):
    print(f"DEBUG: {message}")

# Check if input text is passed via command line argument
if len(sys.argv) < 2:
    print("Usage: python script_name.py '<text_to_summarize>'")
    sys.exit(1)

# Preuzimanje inputa sa komandne linije
input_text = sys.argv[1]

debug_print(f"Input Text: {input_text}")

# Split input text into lines
lines = input_text.splitlines()

debug_print(f"Split lines: {lines}")

# Inicijalizacija LexRank modela
try:
    lxr = LexRank(input_text.splitlines(), stopwords=STOPWORDS['en'])
    debug_print("LexRank model initialized successfully.")
except Exception as e:
    print(f"Error initializing LexRank: {e}")
    sys.exit(1)

# Generisanje sažetka
try:
    summary = lxr.get_summary(lines, summary_size=3)
    debug_print("Summary generated successfully.")
except Exception as e:
    print(f"Error generating summary: {e}")
    sys.exit(1)

# Ispisivanje rezultata
print("Summary:", " ".join(summary))
