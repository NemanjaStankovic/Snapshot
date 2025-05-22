# 📸 Snapshot Bot

Snapshot Bot je Python aplikacija koja omogućava pravljenje snapshot-a (PDF i PNG) web stranica putem Telegram bota. Projekat je razvijen kao rešenje zadatka sa sledećim ciljevima: pravljenje snimka stranice, generisanje kratkog opisa i sumarizacije sadržaja, kao i čuvanje podataka za kasniju pretragu.

---

## 🔧 Korišćene tehnologije

- **Python** – glavni jezik implementacije
- **Playwright** – za pravljenje PNG snapshot-a web stranica
- **Telegram Bot API** – za komunikaciju sa korisnikom
- **SQLite** – kao jednostavna baza za čuvanje podataka
- **Sumy (LexRank)** – za automatsku sumarizaciju sadržaja
- **BeautifulSoup** – za parsiranje HTML sadržaja i ekstrakciju `<meta>` opisa

---

## ✅ Implementirane funkcionalnosti

- ✅ Slanje URL-a putem Telegram bota komandom `/snapshot <URL>`
- ✅ Snapshot stranice (PNG) se pravi pomoću Playwright-a
- ✅ Ekstrakcija opisa stranice iz `<meta name="description">` taga
- ✅ Generisanje sumarizovanog teksta pomoću LexRank metode
- ✅ Čuvanje URL-a, putanja do snapshot-a, opisa i sumarizacije u SQLite bazu
- ✅ Telegram bot odgovara korisniku sa linkovima ka snimku i pratećim informacijama

---

## 🧪 Testiranje

Aplikacija je uspešno testirana na različitim tipovima URL-ova, uključujući statične i dinamičke web stranice.

### Primer poziva komande za kreiranje snapshot-a

![Kreiranje snapshot-a](Pictures/screenshot1)

---

### Primer poziva komande za preuzimanje snapshot-a

![Preuzimanje snapshot-a](Pictures/screenshot2)
