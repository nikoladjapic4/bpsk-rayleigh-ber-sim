# BPSK preko Rejlijevog feding kanala — Monte Karlo simulacija BER-a

Konzolna aplikacija u C#-u koja Monte Karlo metodom procenjuje **verovatnoću greške
po bitu (BER)** za BPSK prenos kroz radio kanal sa Rejlijevim fedingom i belim
Gausovim šumom (AWGN). Simulira se nekoliko scenarija — bez diverzitija i sa
diverzitijem na dve prijemne antene (SC i MRC) — a rezultati se crtaju kao krive
BER / SNR.

## Šta radi

Za svaki scenario i za svaku vrednost SNR-a (0–20 dB) simulacija prolazi kroz ceo
lanac prenosa:

1. **Izvor** — generiše se nasumična binarna sekvenca (jednakoverovatne 0 i 1).
2. **Frejmovi** — podaci se dele na blokove i ispred svakog se dodaje preambula
   od `P` jedinica (koristi se za procenu stanja kanala / CSI).
3. **BPSK modulacija** — bitovi 0/1 se preslikavaju u simbole −1/+1.
4. **Kanal** — signal se množi Rejlijevim feding koeficijentom (blok-feding ili
   vremenski korelisan, preko modifikovanog Džejksovog modela) i dodaje se AWGN
   prema zadatom SNR-u.
5. **Prijem i demodulacija** — kanal se procenjuje iz preambule, signal se
   izjednačava i donosi se odluka o bitu. Podržani su SISO prijem i diverziti
   (SC — biranje najjače grane, MRC — kombinovanje uz maksimalan odnos).
6. **BER** — broje se pogrešni bitovi i računa se verovatnoća greške.

## Scenariji

| Scenario | Brzina | Preambula | Antene | Diverziti |
|----------|:------:|:---------:|:------:|:---------:|
| A | 30 km/h | P = 0 | 1 | — (nema CSI) |
| B | 30 km/h | P = 1 | 1 | — |
| C | 30 km/h | P = 20 | 1 | — |
| D | 130 km/h | P = 20 | 1 | — (vremenski korelisan feding) |
| SC | 30 km/h | P = 20 | 2 | SC |
| MRC | 30 km/h | P = 20 | 2 | MRC |

Parametri sistema (dužina sekveze, bitrate, učestanost nosioca, itd.) nalaze se u
[`SimulationConfig.cs`](BpskRayleighBerSim/SimulationConfig.cs) i lako se menjaju.

## Pokretanje

Potreban je **.NET 10 SDK**.

```bash
cd BpskRayleighBerSim
dotnet run
```

Aplikacija ispisuje BER za svaki SNR u konzoli i pravi:

- `*_BER.csv` — krive BER/SNR za svaki scenario,
- `BER.png` — zbirni grafik svih krivih (logaritamska y-osa).

## Struktura projekta

| Fajl | Uloga |
|------|-------|
| [`Program.cs`](BpskRayleighBerSim/Program.cs) | Ulazna tačka — pokreće sve scenarije i čuva rezultate. |
| [`SimulationConfig.cs`](BpskRayleighBerSim/SimulationConfig.cs) | Parametri sistema i predefinisani scenariji. |
| [`FrameBuilder.cs`](BpskRayleighBerSim/FrameBuilder.cs) | Dodavanje preambule na blokove podataka. |
| [`BpskModulator.cs`](BpskRayleighBerSim/BpskModulator.cs) | BPSK modulacija. |
| [`FaddingChannel.cs`](BpskRayleighBerSim/FaddingChannel.cs) | Rejlijev feding, AWGN i Džejksov simulator. |
| [`Simulator.cs`](BpskRayleighBerSim/Simulator.cs) | Glavna Monte Karlo petlja i demodulatori (SISO / SC / MRC). |
| [`Plot.cs`](BpskRayleighBerSim/Plot.cs) | Crtanje BER/SNR grafika. |

## Tehnologije

C# · .NET 10 · `System.Numerics` (kompleksni brojevi) · [ScottPlot](https://scottplot.net/) za grafike
