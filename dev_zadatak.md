## Zadatak

![[zadatak.png]]
##### **1. Definicija projekta**

- **Cilj**: Napraviti alatku za pravljenje snapshot-a (PDF/PNG) web stranica. (bilo koji stack, dozvoljeno koriscenje LLM-ova)
- **Funkcionalnosti**:
    - Slanje URL-a za koji se pravi snapshot.
    - Snapshot se čuva u bazi podataka.
    - Prikupljanje opisa stranice i generisanje skraćenog teksta (npr. koristeći LexRank ili LSA).
    - Mogućnost pretrage snimljenih podataka (potencijalno korišćenje Elasticsearch-a).
    - Umesto GUI-a, koristiti Telegram bota za slanje komandi i dobijanje rezultata.
    - Rok: 7 dana

##### **2. Tehnologije i alati (!!!PREDLOG!!!)**

- **Biblioteke**:
    - **Playwright**: Za pravljenje snapshot-a (PDF/PNG).
    - **LLM (Large Language Models)**: Za generisanje opisa ili skraćenog teksta (opciono).
    - **LexRank/LSA**: Za sumarizaciju teksta.
- **Baza podataka**: Bilo koja (npr. PostgreSQL, MySQL, Mongo, Elasticsearch, Opensearch) za čuvanje podataka i pretragu.
- **Telegram Bot**: Za interakciju sa korisnikom.
- **Backend**: Bilo koji jezik (npr. Python, Node.js, Java, Golang, Rust, ...).

#### **3. Koraci za implementaciju**

##### **3.1. Backend**

- **3.1.1. Prihvatanje URL-a**:
    - Napraviti API endpoint ili Telegram bot komandu za prihvatanje URL-a.
    - Validacija URL-a prema pravilima (npr. da li je validan URL).

- **3.1.2. Pravljenje snapshot-a**:
    - Koristiti Playwright za pravljenje PDF/PNG snapshot-a.
    - Snapshot sačuvati u bazi.

- **3.1.3. Čuvanje podataka u bazi**:
    - Napraviti model u bazi podataka za čuvanje:
        - URL
        - Putanja do snapshot-a (PDF/PNG)
        - Opis stranice (ako je prikupljen)
        - Skraćeni tekst (ako je generisan)

- **3.1.4. Generisanje opisa i skraćenog teksta**:
	- Prikupiti opis sa HTML stranice
		```html
      <meta name="description" content="encode and decode sensitive information such as passwords to store them in config files - IQooLogic/obfuscator">
		```
    - Koristiti LexRank, LSA ili LLM za generisanje skraćenog teksta.
    - Sačuvati podatke u bazu.

##### **3.2. Telegram bot**

- **3.2.1. Podešavanje bota**:
    - Napraviti Telegram bota koristeći Telegram Bot API.
    - Omogućiti botu da prepoznaje komande (npr. `/snapshot <URL>`).

- **3.2.2. Komunikacija sa backend-om**:
    - Kada bot primi komandu, prosledi URL backend-u.
    - Backend obradi zahtev i vrati rezultat botu.
    - Bot šalje korisniku link do snapshot-a ili drugih podataka.

##### **3.3. Elasticsearch (opciono)**

- **3.3.1. Indeksiranje podataka**:
    - Ako se koristi Elasticsearch, indeksirati opis i skraćeni tekst za lakšu pretragu.

- **3.3.2. Pretraga**:
    - Omogućiti pretragu preko bota ili API-ja.

##### **4. Testiranje (opciono)**

- **4.1. Testiranje funkcionalnosti**:
    - Proveriti da li se snapshot pravi korektno za različite URL-ove.
    - Proveriti da li se podaci čuvaju u bazi.
    - Proveriti da li bot ispravno prepoznaje komande i šalje rezultate.

- **4.2. Testiranje performansi**:
    - Proveriti kako alatka radi sa velikim brojem zahteva.

##### **5. Dokumentacija**

- **5.1. Opis funkcionalnosti**:
    - Napisati kratku dokumentaciju o tome kako alatka radi.

- **5.2. Uputstvo za pokretanje**:
    - Objasniti kako se pokreće backend, bot i ostali delovi sistema.

##### **6. Savet kako pristupiti projektu**

- Prvo implementirati osnovne delove (prihvatanje URL-a, pravljenje snapshot-a, čuvanje u bazu).
- Zatim dodati Telegram bot.
- Na kraju implementirati dodatne funkcionalnosti (generisanje opisa, Elasticsearch).
