# ✈️ Jet Fury Extractor

Extrait le mini-jeu d'avion caché de **Takeover 2** (« Jet Fury », scène `Bonus_Afterburner`)
et en fait un jeu autonome qui démarre directement sur son écran « PRESS START ».

[**⬇️ Télécharger la dernière version**](../../releases/latest)

> Même principe que [9OsGP Extractor](https://github.com/MR-Lox/9OsGP-Extractor) : votre installation n'est jamais modifiée,
> tout est copié dans un nouveau dossier (~5,5 Go), puis un petit module **JetBoot.dll** est déclaré
> dans `ScriptingAssemblies.json` / `RuntimeInitializeOnLoads.json` pour rediriger vers la scène n° 26.

## Utilisation

```powershell
JetFury_Extractor.exe "C:\Chemin\Vers\Takeover 2" "D:\Jeux\JetFury"
```

Sans argument, il cherche le jeu à côté de lui (ou demande le chemin) et crée `JetFury\` à côté du dossier du jeu.
Lancez ensuite `JetFury\JetFury.exe`.

## En jeu

| Touche | Action |
|---|---|
| **F1** | Ouvrir / fermer le menu d'options (le jeu se met en pause) |
| **Échap** | Fermer le menu d'options, sinon proposer de quitter le jeu |

### Menu d'options

- **Affichage** : langue (English, Español, Français, Deutsch, Português BR), plein écran / plein écran fenêtré / fenêtré, résolution, V-Sync.
- **Graphismes** : bezel de borne d'arcade, effet écran CRT, contours noirs (cel shading), post-traitement BD, trame BD sur les éclairages, anticrénelage (origine, aucun, FXAA, SMAA, TAA).
- **HUD** : le HUD d'origine est prévu pour le 4:3 et sort de l'écran en 16:9 / 21:9 ; l'option « Garder tout le HUD à l'écran » (activée par défaut) le ramène dans l'image. Réglages de taille, d'écartement horizontal et vertical (à 1, blocs plaqués contre les bords, ou contre l'écran du bezel) et de marge, et affichage bloc par bloc : temps, jets abattus, frégates coulées, vies, altitude, aide des boutons, viseur, messages, instruments de vol, cadres de visée, blocs du mode course.
- **Audio** : volumes général, musique, moteurs des avions et effets. Bande-son d'origine ou vos fichiers mp3 / ogg / wav placés dans un dossier `Music` à côté de `JetFury.exe` (un morceau ou tous au hasard).
- **Contrôles** : réassignation clavier et manette, avec le rôle de chaque action dans Jet Fury (virer, manche, postcombustion, mitrailleuse, missiles, leurres, esquive…), et retour aux valeurs par défaut.

Les réglages sont enregistrés dans `JetFury_settings.ini`, à côté de `JetFury.exe`.

## Compiler

```powershell
dotnet build JetBoot -c Release -o JetBoot/out -p:GameManaged="C:\Chemin\Vers\Takeover 2\Takeover2_Data\Managed"
Copy-Item JetBoot/out/JetBoot.dll Extractor/Resources/
dotnet publish Extractor -c Release -o dist
```

## Outils de développement (dans le dossier extrait)

- `dump_enabled.txt` : écrit `scene_dump.txt` (hiérarchie, FSM PlayMaker, Rewired, textes UI) 8 s après le chargement.
- `testbench.txt` : captures dans `shots\` (ligne 1 : nombre, ligne 2 : intervalle en s, ligne 3 : `start` pour lancer une partie).

## Avertissement

Projet de fan, non officiel. Aucun contenu du jeu n'est distribué : il faut posséder Takeover 2. Code sous licence MIT.
