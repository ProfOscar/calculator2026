# Revisione di `FormMain.cs`

Ho letto `FormMain.cs` riga per riga e ho seguito a mano le sequenze di tasti più comuni. Non ho modificato nessun file e non ho eseguito il programma. Ho riportato solo i problemi che portano a un comportamento sbagliato o diverso da Windows 10, ordinati per gravità. In fondo ci sono poche ottimizzazioni semplici.

---

## ?? Bug gravi

### 1. I risultati lunghi vengono tagliati e il valore cambia (`ResultLabel_TextChanged`, righe 122?123)
```csharp
if (resultLabel.Text.Length > 16)
    resultLabel.Text = resultLabel.Text.Substring(0, 16);
```
Il taglio vale per qualsiasi testo, compresi i risultati. Per esempio, `99999999 x 99999999 =` dà `9.999.999.800.000.001`. Il testo diventa `9.999.999.800.00`, cioè circa **10 miliardi invece di 10 trilioni**. Dato che il calcolo successivo rilegge l'operando dall'etichetta, l'errore si trasmette anche ai conti seguenti.

**Soluzione:** togliere il taglio da `TextChanged` e limitare a 16 cifre solo la digitazione, come fa Windows. Il caso `Number` diventa:
```diff
 case SymbolType.Number:
     if (lastBtnStruct.Type == SymbolType.EqualSign)
         ClearAll();
     if (resultLabel.Text == "0" || lastBtnStruct.Type == SymbolType.Operator)
         resultLabel.Text = "";
-    resultLabel.Text += clickedButtonStruct.Content;
+    if (resultLabel.Text.Replace(".", "").Replace(",", "").Replace("-", "").Length < 16)
+        resultLabel.Text += clickedButtonStruct.Content;
     break;
```
```diff
-            if (resultLabel.Text.Length > 16)
-                resultLabel.Text = resultLabel.Text.Substring(0, 16);
```
Così alcuni risultati molto lunghi (per esempio `1/3`) supereranno le dimensioni dell'etichetta. Per restare semplici si può arrotondare il risultato prima di mostrarlo, per esempio con `Math.Round(result, 10)`. La notazione scientifica di Windows è fuori portata per questo progetto.

### 2. Dipendenza dalla lingua del PC (righe 109, 246, 312, 317, 301, 347)
L'etichetta viene formattata sempre in italiano (`it-IT`), ma `decimal.Parse(...)` e `ToString()` usano la **lingua impostata sul PC**. Su un PC in inglese, `"1.234,5"` viene letto male oppure provoca un'eccezione, e il programma si chiude. La virgola del tasto `,` è scritta fissa nel codice, quindi il problema è reale.

**Soluzione:** creare la cultura una sola volta come campo e usarla ovunque.
```csharp
private static readonly CultureInfo IT = new CultureInfo("it-IT");
...
decimal.Parse(resultLabel.Text, IT)
operand2.ToString(IT)   /   result.ToString(IT)
```
In alternativa basta una riga in `Program.Main`, prima di `Application.Run`:
```csharp
System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("it-IT");
```
La seconda strada è la più semplice da spiegare in classe.

### 3. `=` subito dopo un operatore non calcola niente (`ManageOperator`, riga 307)
Con `5 + =` Windows mostra `10`. Qui `lastBtnStruct.Type == Operator`, quindi tutto il blocco viene saltato. L'etichetta mostra `5 + 0 =` (con un `operand2` vecchio) e il risultato non cambia.
```diff
-            if (lastBtnStruct.Type != SymbolType.Operator)
+            if (lastBtnStruct.Type != SymbolType.Operator || clickedButtonStruct.Type == SymbolType.EqualSign)
```
Con questa modifica, `operand2` viene letto dall'etichetta (5) e il conto `5 + 5` si esegue normalmente.

### 4. Le cifre si accodano a un risultato invece di sostituirlo (righe 189 e 201)
- Con `9 ?` si ottiene `3`. Se poi si digita `5`, compare `35` invece di `5`.
- Con `5 + ,` compare `5,` invece di `0,`. Lo stesso succede con `,` subito dopo `=`, che si accoda al risultato.

**Soluzione:**
```diff
 case SymbolType.Number:
     ...
-    if (resultLabel.Text == "0" || lastBtnStruct.Type == SymbolType.Operator)
+    if (resultLabel.Text == "0" || lastBtnStruct.Type == SymbolType.Operator
+        || lastBtnStruct.Type == SymbolType.SpecialOperator)
         resultLabel.Text = "";
 ...
 case SymbolType.DecimalPoint:
+    if (lastBtnStruct.Type == SymbolType.EqualSign)
+        ClearAll();
+    if (lastBtnStruct.Type == SymbolType.Operator || lastBtnStruct.Type == SymbolType.SpecialOperator)
+        resultLabel.Text = "0";
     if (!resultLabel.Text.Contains(","))
         resultLabel.Text += clickedButtonStruct.Content;
     break;
```

### 5. Il tasto ? cancella anche i risultati (riga 211)
Con `5 + 3 =` e poi ?, il risultato `8` diventa `0`. Con `5 +` e poi ?, sparisce il primo operando: l'ultimo tasto diventa `Backspace`, quindi la cifra successiva si accoda (`5 + ? 6` dà `56`). Windows invece ignora ? in questi casi.
```diff
 case SymbolType.Backspace:
+    if (lastBtnStruct.Type == SymbolType.Operator || lastBtnStruct.Type == SymbolType.EqualSign
+        || lastBtnStruct.Type == SymbolType.SpecialOperator)
+        return;   // ignora il tasto: lastBtnStruct resta invariato
     resultLabel.Text = resultLabel.Text.Substring(0, resultLabel.Text.Length - 1);
```
Il `return` serve apposta: non aggiorna `lastBtnStruct`, così il programma "ricorda" ancora l'operatore.

---

## ?? Bug medi

### 6. Il programma si chiude se il numero diventa troppo grande
Il tipo `decimal` arriva al massimo a circa 7,9·10²?. Basta usare `x²` qualche volta di seguito, o fare una moltiplicazione grande, per ottenere una `OverflowException` e la chiusura del programma. Il bug 1 oggi nasconde in parte il problema, ma dopo averlo corretto diventa più frequente. Si può usare un `try/catch` semplice, coerente con lo stile dei messaggi già presenti:
```csharp
try { result = operand1 * operand2; }
catch (OverflowException) { infoLabel.Text = "Overflow"; return; }
```
Lo stesso vale per `operand2 * operand2` nel tasto `x²`.

### 7. L'etichetta `=` è sbagliata se non c'è un'operazione in corso (riga 352)
Con `5 =` compare `"5   0 ="`. Windows mostra `"5 ="`.
```diff
 if (clickedButtonStruct.Type == SymbolType.EqualSign)
 {
-    infoLabel.Text = $"{lastOperand1} {lastOperator} {operand2} =";
+    if (lastOperator == ' ')
+        infoLabel.Text = $"{operand1} =";
+    else
+        infoLabel.Text = $"{lastOperand1} {lastOperator} {operand2} =";
 }
```

### 8. Un'operazione speciale dopo `=` si accoda alla riga sopra (riga 242)
Con `5 + 3 = ?` l'etichetta mostra `"5 + 3 = ?(8)"`.
```diff
-            if (lastOperator == ' ')
+            if (lastOperator == ' ' || lastBtnStruct.Type == SymbolType.EqualSign)
             {
                 infoLabel.Text = "";
             }
```

### 9. `%` senza operazione in corso
In Windows, `50 %` senza un operatore prima dà `0`. Qui il numero resta invariato. È una differenza piccola: basta aggiungere `default: operand2 = 0; break;` nello `switch (lastOperator)` del caso `'%'`.

---

## ?? Problemi minori (da sapere, non indispensabili)
- **Messaggi di errore con `Task.Delay`:** se si preme un altro tasto durante i 2 secondi di attesa, alla fine l'etichetta torna al testo vecchio e cancella quello nuovo. Per un progetto didattico si può lasciare così.
- **Zeri finali nei risultati:** `2,50 x 2 =` mostra `5,00`, mentre Windows mostra `5`. Il motivo è che `decimal` conserva il numero di decimali. Si risolve con `result.ToString("0.############################")`, da usare insieme al punto 2.
- **`ClearEntry` dopo `=`:** in Windows si comporta come `C`. Qui azzera solo il numero visualizzato.

---

## ?? Ottimizzazioni semplici
1. **`ResultLabel_TextChanged`:** crea una nuova `CultureInfo` a ogni carattere e un nuovo `Font` a ogni modifica (i font sono risorse da liberare). Conviene usare la cultura come campo `static readonly` (vedi punto 2) e cambiare il font solo quando la dimensione cambia davvero.
2. **Assegnare `resultLabel.Text` dentro il suo stesso `TextChanged`** fa scattare di nuovo l'evento. Funziona perché al secondo giro il testo non cambia più, ma è utile spiegarlo agli studenti per evitare che sembri "magia".
3. **`Location = new Point(-20, ...)` con `Size = this.Width`:** è un trucco per compensare la larghezza del bordo della finestra. È più chiaro usare `Location = new Point(0, ...)` con `Size = new Size(this.ClientSize.Width, ...)`.

---

## Ordine consigliato per le correzioni
1. Punto **2** (lingua del PC): serve anche per gli altri punti.
2. Punti **1, 3, 4, 5**: sono i bug che gli studenti noteranno subito usando la calcolatrice.
3. Punti **6, 7, 8**.
4. Gli altri restano facoltativi.
