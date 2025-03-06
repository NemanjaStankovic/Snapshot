import sys
sys.stdout.reconfigure(encoding='utf-8')
from lexrank import LexRank
from lexrank import STOPWORDS


if len(sys.argv) < 2:
    sys.exit(1)
input_text = sys.argv[1]
lines = input_text.splitlines()

try:
    lxr = LexRank(input_text.splitlines(), stopwords=STOPWORDS['en'])
except Exception as e:
    print(f"{e}")
    sys.exit(1)

try:
    summary = lxr.get_summary(lines, summary_size=3)
except Exception as e:
    print(f"{e}")
    sys.exit(1)

print("".join(summary))
