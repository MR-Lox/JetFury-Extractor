using System.Collections.Generic;
using UnityEngine;

namespace JetBoot
{
    // Languages of the mod's own interface: the same five as Takeover 2's options menu.
    // Texts are written in French in the code; L.T() returns the translation for the chosen language.
    public static class L
    {
        public static readonly string[] Codes = { "en", "es", "fr", "de", "pt" };
        public static readonly string[] Names = { "English", "Español", "Français", "Deutsch", "Português BR" };

        public static string Code = "fr";

        public static string Default()
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.French: return "fr";
                case SystemLanguage.Spanish: return "es";
                case SystemLanguage.German: return "de";
                case SystemLanguage.Portuguese: return "pt";
                default: return "en";
            }
        }

        public static string T(string fr)
        {
            if (Code == "fr" || !Table.TryGetValue(fr, out var t)) return fr;
            switch (Code)
            {
                case "en": return t[0];
                case "es": return t[1];
                case "de": return t[2];
                case "pt": return t[3];
                default: return fr;
            }
        }

        // French -> English, Spanish, German, Portuguese (Brazil).
        static readonly Dictionary<string, string[]> Table = new Dictionary<string, string[]>
        {
            { "arrêté", new[] { "stopped", "detenido", "angehalten", "parado" } },
            { "F1 : options   —   C / bouton Y : changer de vue", new[] { "F1: options   —   C / Y button: change view", "F1: opciones   —   C / botón Y: cambiar de vista", "F1: Optionen   —   C / Y-Taste: Ansicht wechseln", "F1: opções   —   C / botão Y: mudar a visão" } },
            { "Affichage", new[] { "Display", "Pantalla", "Anzeige", "Tela" } },
            { "Graphismes", new[] { "Graphics", "Gráficos", "Grafik", "Gráficos" } },
            { "Caméra", new[] { "Camera", "Cámara", "Kamera", "Câmera" } },
            { "HUD", new[] { "HUD", "HUD", "HUD", "HUD" } },
            { "Contrôles", new[] { "Controls", "Controles", "Steuerung", "Controles" } },
            { "Fermer (F1 / Échap)", new[] { "Close (F1 / Esc)", "Cerrar (F1 / Esc)", "Schließen (F1 / Esc)", "Fechar (F1 / Esc)" } },
            { "Quitter le jeu", new[] { "Quit game", "Salir del juego", "Spiel beenden", "Sair do jogo" } },
            { "Oui", new[] { "Yes", "Sí", "Ja", "Sim" } },
            { "Non", new[] { "No", "No", "Nein", "Não" } },
            { "Mode d'affichage", new[] { "Display mode", "Modo de pantalla", "Anzeigemodus", "Modo de exibição" } },
            { "Plein écran", new[] { "Fullscreen", "Pantalla completa", "Vollbild", "Tela cheia" } },
            { "Plein écran fenêtré", new[] { "Borderless window", "Ventana sin bordes", "Randloses Fenster", "Janela sem bordas" } },
            { "Fenêtré", new[] { "Windowed", "Ventana", "Fenster", "Janela" } },
            { "Résolution", new[] { "Resolution", "Resolución", "Auflösung", "Resolução" } },
            { "Appliquer", new[] { "Apply", "Aplicar", "Anwenden", "Aplicar" } },
            { "Actuelle : ", new[] { "Current: ", "Actual: ", "Aktuell: ", "Atual: " } },
            { "Synchronisation verticale (V-Sync)", new[] { "Vertical sync (V-Sync)", "Sincronización vertical (V-Sync)", "Vertikale Synchronisation (V-Sync)", "Sincronização vertical (V-Sync)" } },
            { "Vue", new[] { "View", "Vista", "Ansicht", "Visão" } },
            { "En course : touche C (clavier) ou bouton Y / Select (manette) pour changer de vue.", new[] { "In race: C key (keyboard) or Y / Select button (gamepad) to change view.", "En carrera: tecla C (teclado) o botón Y / Select (mando) para cambiar de vista.", "Im Rennen: Taste C (Tastatur) oder Y- / Select-Taste (Gamepad) zum Wechseln der Ansicht.", "Na corrida: tecla C (teclado) ou botão Y / Select (controle) para mudar a visão." } },
            { "Replay de la course sur l'écran « PRESS START »", new[] { "Race replay on the \"PRESS START\" screen", "Repetición de la carrera en la pantalla \"PRESS START\"", "Rennwiederholung auf dem „PRESS START“-Bildschirm", "Replay da corrida na tela \"PRESS START\"" } },
            { "F9 sur l'écran « PRESS START » : revoir le dernier replay (gardé même après avoir quitté le jeu).", new[] { "F9 on the \"PRESS START\" screen: watch the last replay again (kept even after quitting the game).", "F9 en la pantalla \"PRESS START\": volver a ver la última repetición (se conserva al salir del juego).", "F9 auf dem „PRESS START“-Bildschirm: letzte Wiederholung erneut ansehen (bleibt auch nach Spielende erhalten).", "F9 na tela \"PRESS START\": rever o último replay (mantido mesmo após sair do jogo)." } },
            { "Réglages des vues intérieures", new[] { "Interior view settings", "Ajustes de las vistas interiores", "Einstellungen der Innenansichten", "Ajustes das visões internas" } },
            { "Champ de vision", new[] { "Field of view", "Campo de visión", "Sichtfeld", "Campo de visão" } },
            { "Hauteur", new[] { "Height", "Altura", "Höhe", "Altura" } },
            { "Avant / arrière", new[] { "Forward / back", "Adelante / atrás", "Vor / zurück", "Frente / trás" } },
            { "Inclinaison", new[] { "Tilt", "Inclinación", "Neigung", "Inclinação" } },
            { "Valeurs par défaut", new[] { "Default values", "Valores por defecto", "Standardwerte", "Valores padrão" } },
            { "Éléments affichés", new[] { "Displayed elements", "Elementos mostrados", "Angezeigte Elemente", "Elementos exibidos" } },
            { "Mini-carte (cachée par les développeurs)", new[] { "Minimap (hidden by the developers)", "Minimapa (oculto por los desarrolladores)", "Minikarte (von den Entwicklern ausgeblendet)", "Minimapa (oculto pelos desenvolvedores)" } },
            { "Compte-tours et vitesse", new[] { "Rev counter and speed", "Cuentarrevoluciones y velocidad", "Drehzahlmesser und Geschwindigkeit", "Conta-giros e velocidade" } },
            { "Taille", new[] { "Size", "Tamaño", "Größe", "Tamanho" } },
            { "Position horizontale", new[] { "Horizontal position", "Posición horizontal", "Horizontale Position", "Posição horizontal" } },
            { "Position verticale", new[] { "Vertical position", "Posición vertical", "Vertikale Position", "Posição vertical" } },
            { "En bas de l'écran en vue capot / intérieure", new[] { "At the bottom of the screen in hood / interior view", "Abajo de la pantalla en vista capó / interior", "Unten am Bildschirm in Motorhauben- / Innenansicht", "Na parte de baixo da tela na visão capô / interna" } },
            { "Autres informations (temps, position, tours)", new[] { "Other information (time, position, laps)", "Otra información (tiempo, posición, vueltas)", "Weitere Infos (Zeit, Position, Runden)", "Outras informações (tempo, posição, voltas)" } },
            { "Écartement horizontal", new[] { "Horizontal spread", "Separación horizontal", "Horizontaler Abstand", "Afastamento horizontal" } },
            { "Écartement vertical", new[] { "Vertical spread", "Separación vertical", "Vertikaler Abstand", "Afastamento vertical" } },
            { "Marge aux bords", new[] { "Edge margin", "Margen a los bordes", "Randabstand", "Margem das bordas" } },
            { "Écartement à 1 : blocs plaqués contre les bords de l'écran (utile en 16:9 / 21:9). Hauteur positive = plus haut.", new[] { "Spread at 1: blocks pushed against the screen edges (useful in 16:9 / 21:9). Positive height = higher.", "Separación a 1: bloques pegados a los bordes de la pantalla (útil en 16:9 / 21:9). Altura positiva = más arriba.", "Abstand 1: Blöcke an den Bildschirmrand gedrückt (nützlich bei 16:9 / 21:9). Positive Höhe = höher.", "Afastamento em 1: blocos encostados nas bordas da tela (útil em 16:9 / 21:9). Altura positiva = mais alto." } },
            { "Bezel (cadre de la borne d'arcade)", new[] { "Bezel (arcade cabinet frame)", "Bezel (marco de la máquina recreativa)", "Bezel (Rahmen des Arcade-Automaten)", "Bezel (moldura do fliperama)" } },
            { "Effet écran CRT (courbure, lignes)", new[] { "CRT screen effect (curvature, lines)", "Efecto pantalla CRT (curvatura, líneas)", "CRT-Bildschirmeffekt (Krümmung, Linien)", "Efeito de tela CRT (curvatura, linhas)" } },
            { "Contours noirs (cel shading)", new[] { "Black outlines (cel shading)", "Contornos negros (cel shading)", "Schwarze Konturen (Cel-Shading)", "Contornos pretos (cel shading)" } },
            { "Post-traitement BD (trame, couleurs)", new[] { "Comic post-processing (halftone, colours)", "Posprocesado cómic (trama, colores)", "Comic-Nachbearbeitung (Raster, Farben)", "Pós-processamento HQ (retícula, cores)" } },
            { "Trame BD sur les éclairages (halftone)", new[] { "Comic halftone on the lights", "Trama cómic en las luces (halftone)", "Comic-Raster auf den Lichtern (Halftone)", "Retícula HQ nas luzes (halftone)" } },
            { "Les changements sont appliqués immédiatement et enregistrés à la fermeture du menu.", new[] { "Changes are applied immediately and saved when the menu is closed.", "Los cambios se aplican al instante y se guardan al cerrar el menú.", "Änderungen werden sofort übernommen und beim Schließen des Menüs gespeichert.", "As alterações são aplicadas na hora e salvas ao fechar o menu." } },
            { "Aucun volant / contrôleur détecté. Branchez-le puis rouvrez ce menu.", new[] { "No wheel / controller detected. Plug it in, then reopen this menu.", "No se detecta ningún volante / controlador. Conéctalo y vuelve a abrir este menú.", "Kein Lenkrad / Controller erkannt. Anschließen und dieses Menü erneut öffnen.", "Nenhum volante / controle detectado. Conecte-o e reabra este menu." } },
            { "Périphérique", new[] { "Device", "Dispositivo", "Gerät", "Dispositivo" } },
            { "Configuré pour ce périphérique.", new[] { "Configured for this device.", "Configurado para este dispositivo.", "Für dieses Gerät eingerichtet.", "Configurado para este dispositivo." } },
            { "Pas encore configuré : lancez l'assistant.", new[] { "Not configured yet: run the wizard.", "Aún sin configurar: inicia el asistente.", "Noch nicht eingerichtet: Assistent starten.", "Ainda não configurado: inicie o assistente." } },
            { "Relancer l'assistant de configuration", new[] { "Run the setup wizard again", "Volver a iniciar el asistente", "Einrichtungsassistent erneut starten", "Reiniciar o assistente de configuração" } },
            { "Lancer l'assistant de configuration", new[] { "Run the setup wizard", "Iniciar el asistente de configuración", "Einrichtungsassistent starten", "Iniciar o assistente de configuração" } },
            { "Axes en direct", new[] { "Live axes", "Ejes en directo", "Achsen live", "Eixos ao vivo" } },
            { " (direction)", new[] { " (steering)", " (dirección)", " (Lenkung)", " (direção)" } },
            { " (accélérateur)", new[] { " (throttle)", " (acelerador)", " (Gas)", " (acelerador)" } },
            { " (frein)", new[] { " (brake)", " (freno)", " (Bremse)", " (freio)" } },
            { "Réglages", new[] { "Settings", "Ajustes", "Einstellungen", "Ajustes" } },
            { "Sensibilité de la direction", new[] { "Steering sensitivity", "Sensibilidad de la dirección", "Lenkempfindlichkeit", "Sensibilidade da direção" } },
            { "Zone morte des pédales", new[] { "Pedal dead zone", "Zona muerta de los pedales", "Pedal-Totzone", "Zona morta dos pedais" } },
            { "Sensibilité 1 = braquage maximal à fond de volant ; 2 = à mi-course (utile pour les volants 900°).", new[] { "Sensitivity 1 = full lock at full wheel turn; 2 = at half turn (useful for 900° wheels).", "Sensibilidad 1 = giro máximo con el volante a tope; 2 = a mitad de recorrido (útil para volantes de 900°).", "Empfindlichkeit 1 = voller Einschlag bei vollem Lenkradeinschlag; 2 = bei halbem (nützlich für 900°-Lenkräder).", "Sensibilidade 1 = esterço máximo com o volante no fim; 2 = na metade (útil para volantes de 900°)." } },
            { "Inverser la direction", new[] { "Invert steering", "Invertir la dirección", "Lenkung umkehren", "Inverter a direção" } },
            { "Oublier ce volant", new[] { "Forget this wheel", "Olvidar este volante", "Dieses Lenkrad vergessen", "Esquecer este volante" } },
            { "Retour de force", new[] { "Force feedback", "Force feedback", "Force Feedback", "Force feedback" } },
            { "Activer le retour de force", new[] { "Enable force feedback", "Activar el force feedback", "Force Feedback aktivieren", "Ativar o force feedback" } },
            { "Force générale", new[] { "Overall strength", "Fuerza general", "Gesamtstärke", "Força geral" } },
            { "Rappel au centre", new[] { "Centring force", "Retorno al centro", "Zentrierkraft", "Retorno ao centro" } },
            { "Glisse et chocs", new[] { "Slides and impacts", "Derrapes y choques", "Rutschen und Stöße", "Derrapagens e batidas" } },
            { "Inverser le sens des forces", new[] { "Invert force direction", "Invertir el sentido de las fuerzas", "Kraftrichtung umkehren", "Inverter o sentido das forças" } },
            { "État : ", new[] { "Status: ", "Estado: ", "Status: ", "Estado: " } },
            { "1/6 — Position de repos", new[] { "1/6 — Rest position", "1/6 — Posición de reposo", "1/6 — Ruheposition", "1/6 — Posição de repouso" } },
            { "Volant au centre, pieds levés des pédales, puis cliquez sur Valider.", new[] { "Wheel centred, feet off the pedals, then click Confirm.", "Volante centrado, pies fuera de los pedales y pulsa Validar.", "Lenkrad mittig, Füße von den Pedalen, dann auf Bestätigen klicken.", "Volante no centro, pés fora dos pedais, depois clique em Confirmar." } },
            { "2/6 — Tournez le volant à fond vers la DROITE", new[] { "2/6 — Turn the wheel fully to the RIGHT", "2/6 — Gira el volante a tope a la DERECHA", "2/6 — Lenkrad ganz nach RECHTS drehen", "2/6 — Gire o volante todo para a DIREITA" } },
            { "3/6 — Enfoncez l'ACCÉLÉRATEUR à fond", new[] { "3/6 — Press the THROTTLE fully", "3/6 — Pisa el ACELERADOR a fondo", "3/6 — GAS voll durchtreten", "3/6 — Pise o ACELERADOR até o fim" } },
            { "4/6 — Enfoncez le FREIN à fond", new[] { "4/6 — Press the BRAKE fully", "4/6 — Pisa el FRENO a fondo", "4/6 — BREMSE voll durchtreten", "4/6 — Pise o FREIO até o fim" } },
            { "Maintenez la position, puis cliquez sur Valider.", new[] { "Hold the position, then click Confirm.", "Mantén la posición y pulsa Validar.", "Position halten, dann auf Bestätigen klicken.", "Mantenha a posição e clique em Confirmar." } },
            { "   Détecté : ", new[] { "   Detected: ", "   Detectado: ", "   Erkannt: ", "   Detectado: " } },
            { "   (en attente d'un mouvement…)", new[] { "   (waiting for a movement…)", "   (esperando un movimiento…)", "   (warte auf eine Bewegung…)", "   (aguardando um movimento…)" } },
            { "5/6 — Bouton « changer de vue »", new[] { "5/6 — \"Change view\" button", "5/6 — Botón \"cambiar de vista\"", "5/6 — Taste „Ansicht wechseln“", "5/6 — Botão \"mudar a visão\"" } },
            { "6/6 — Bouton « menu des options »", new[] { "6/6 — \"Options menu\" button", "6/6 — Botón \"menú de opciones\"", "6/6 — Taste „Optionsmenü“", "6/6 — Botão \"menu de opções\"" } },
            { "Appuyez sur le bouton du volant à utiliser, ou cliquez sur Passer.", new[] { "Press the wheel button to use, or click Skip.", "Pulsa el botón del volante que quieras usar, o pulsa Omitir.", "Gewünschte Lenkradtaste drücken oder auf Überspringen klicken.", "Aperte o botão do volante a usar, ou clique em Pular." } },
            { "Valider", new[] { "Confirm", "Validar", "Bestätigen", "Confirmar" } },
            { "Passer", new[] { "Skip", "Omitir", "Überspringen", "Pular" } },
            { "Annuler", new[] { "Cancel", "Cancelar", "Abbrechen", "Cancelar" } },
            { "Assigné : ", new[] { "Assigned: ", "Asignado: ", "Zugewiesen: ", "Atribuído: " } },
            { "Annulé", new[] { "Cancelled", "Cancelado", "Abgebrochen", "Cancelado" } },
            { "Système de contrôles (Rewired) non prêt.", new[] { "Control system (Rewired) not ready.", "Sistema de controles (Rewired) no disponible.", "Steuerungssystem (Rewired) nicht bereit.", "Sistema de controles (Rewired) não está pronto." } },
            { "Clavier", new[] { "Keyboard", "Teclado", "Tastatur", "Teclado" } },
            { "Manette", new[] { "Gamepad", "Mando", "Gamepad", "Controle" } },
            { "Volant / pédalier", new[] { "Wheel / pedals", "Volante / pedales", "Lenkrad / Pedale", "Volante / pedais" } },
            { "Aucune manette détectée.", new[] { "No gamepad detected.", "No se detecta ningún mando.", "Kein Gamepad erkannt.", "Nenhum controle detectado." } },
            { "Appuyez sur la touche / le bouton à assigner… (6 s)", new[] { "Press the key / button to assign… (6 s)", "Pulsa la tecla / el botón que quieras asignar… (6 s)", "Taste / Knopf zum Zuweisen drücken… (6 s)", "Aperte a tecla / o botão a atribuir… (6 s)" } },
            { "Aucune configuration pour ce périphérique.", new[] { "No configuration for this device.", "No hay configuración para este dispositivo.", "Keine Konfiguration für dieses Gerät.", "Nenhuma configuração para este dispositivo." } },
            { "Carte ", new[] { "Map ", "Mapa ", "Belegung ", "Mapa " } },
            { "Rétablir les contrôles par défaut", new[] { "Restore default controls", "Restablecer los controles por defecto", "Standardsteuerung wiederherstellen", "Restaurar os controles padrão" } },
            { "Contrôles par défaut rétablis", new[] { "Default controls restored", "Controles por defecto restablecidos", "Standardsteuerung wiederhergestellt", "Controles padrão restaurados" } },
            { "Cliquez sur une touche pour la changer. Les contrôles sont partagés avec Takeover 2.", new[] { "Click a key to change it. Controls are shared with Takeover 2.", "Haz clic en una tecla para cambiarla. Los controles se comparten con Takeover 2.", "Auf eine Taste klicken, um sie zu ändern. Die Steuerung wird mit Takeover 2 geteilt.", "Clique numa tecla para mudá-la. Os controles são compartilhados com Takeover 2." } },
            { "Impossible de démarrer l'assignation", new[] { "Could not start the assignment", "No se pudo iniciar la asignación", "Zuweisung konnte nicht gestartet werden", "Não foi possível iniciar a atribuição" } },
            { "désactivé", new[] { "disabled", "desactivado", "deaktiviert", "desativado" } },
            { "erreur : ", new[] { "error: ", "error: ", "Fehler: ", "erro: " } },
            { "aucun volant à retour de force détecté", new[] { "no force feedback wheel detected", "no se detecta ningún volante con force feedback", "kein Lenkrad mit Force Feedback erkannt", "nenhum volante com force feedback detectado" } },
            { "actif sur ", new[] { "active on ", "activo en ", "aktiv auf ", "ativo em " } },
            { "Poursuite (origine)", new[] { "Chase (original)", "Persecución (original)", "Verfolger (Original)", "Perseguição (original)" } },
            { "Capot", new[] { "Hood", "Capó", "Motorhaube", "Capô" } },
            { "Intérieure sans capot", new[] { "Interior without hood", "Interior sin capó", "Innen ohne Motorhaube", "Interna sem capô" } },
            { "Temps restant", new[] { "Time left", "Tiempo restante", "Verbleibende Zeit", "Tempo restante" } },
            { "Position", new[] { "Position", "Posición", "Position", "Posição" } },
            { "Tour", new[] { "Lap", "Vuelta", "Runde", "Volta" } },
            { "Temps au tour", new[] { "Lap times", "Tiempos por vuelta", "Rundenzeiten", "Tempos de volta" } },
            { "Messages (départ, checkpoint, contresens…)", new[] { "Messages (start, checkpoint, wrong way…)", "Mensajes (salida, checkpoint, sentido contrario…)", "Meldungen (Start, Checkpoint, falsche Richtung…)", "Mensagens (largada, checkpoint, contramão…)" } },
            { "Langue", new[] { "Language", "Idioma", "Sprache", "Idioma" } },
            { "Présentation de la voiture avant le compte à rebours", new[] { "Car presentation before the countdown", "Presentación del coche antes de la cuenta atrás", "Fahrzeugvorstellung vor dem Countdown", "Apresentação do carro antes da contagem regressiva" } },
            { "Audio", new[] { "Audio", "Audio", "Audio", "Áudio" } },
            { "Volumes", new[] { "Volume", "Volumen", "Lautstärke", "Volume" } },
            { "Général", new[] { "Master", "General", "Gesamt", "Geral" } },
            { "Musique", new[] { "Music", "Música", "Musik", "Música" } },
            { "Moteurs", new[] { "Engines", "Motores", "Motoren", "Motores" } },
            { "Effets sonores", new[] { "Sound effects", "Efectos de sonido", "Soundeffekte", "Efeitos sonoros" } },
            { "Bande-son", new[] { "Soundtrack", "Banda sonora", "Soundtrack", "Trilha sonora" } },
            { "Originale (90's GP)", new[] { "Original (90's GP)", "Original (90's GP)", "Original (90's GP)", "Original (90's GP)" } },
            { "Aléatoire (vos musiques)", new[] { "Random (your music)", "Aleatoria (tu música)", "Zufällig (eigene Musik)", "Aleatória (suas músicas)" } },
            { "Vos propres musiques (mp3, ogg, wav) : placez-les dans le dossier « Music » à côté de JetFury.exe.", new[] { "Your own music (mp3, ogg, wav): put it in the \"Music\" folder next to JetFury.exe.", "Tu propia música (mp3, ogg, wav): colócala en la carpeta \"Music\" junto a JetFury.exe.", "Eigene Musik (mp3, ogg, wav): in den Ordner „Music“ neben JetFury.exe legen.", "Suas próprias músicas (mp3, ogg, wav): coloque-as na pasta \"Music\" ao lado de JetFury.exe." } },
            { "Quitter le jeu ?", new[] { "Quit the game?", "¿Salir del juego?", "Spiel beenden?", "Sair do jogo?" } },
            { "Quitter", new[] { "Quit", "Salir", "Beenden", "Sair" } },
            { "Entrée : quitter   —   Échap : annuler", new[] { "Enter: quit   —   Esc: cancel", "Intro: salir   —   Esc: cancelar", "Eingabe: beenden   —   Esc: abbrechen", "Enter: sair   —   Esc: cancelar" } },
            { "Originale (Jet Fury)", new[] { "Original (Jet Fury)", "Original (Jet Fury)", "Original (Jet Fury)", "Original (Jet Fury)" } },
            { "F1 : options   —   Échap : quitter", new[] { "F1: options   —   Esc: quit", "F1: opciones   —   Esc: salir", "F1: Optionen   —   Esc: beenden", "F1: opções   —   Esc: sair" } },
            { "Effet écran CRT (courbure de l'écran)", new[] { "CRT screen effect (screen curvature)", "Efecto de pantalla CRT (curvatura)", "CRT-Bildschirmeffekt (Wölbung)", "Efeito de tela CRT (curvatura)" } },
            { "Anticrénelage", new[] { "Anti-aliasing", "Antialiasing", "Kantenglättung", "Antisserrilhamento" } },
            { "Jeu d'origine", new[] { "Game default", "Por defecto", "Spielstandard", "Padrão do jogo" } },
            { "Aucun", new[] { "None", "Ninguno", "Keine", "Nenhum" } },
            { "Moteurs (avions)", new[] { "Engines (jets)", "Motores (aviones)", "Triebwerke (Jets)", "Motores (aviões)" } },
            { "Virer à droite", new[] { "Turn right", "Girar a la derecha", "Rechts kurven", "Virar à direita" } },
            { "Virer à gauche", new[] { "Turn left", "Girar a la izquierda", "Links kurven", "Virar à esquerda" } },
            { "Virer (gauche / droite)", new[] { "Turn (left / right)", "Girar (izquierda / derecha)", "Kurven (links / rechts)", "Virar (esquerda / direita)" } },
            { "Manche vers le haut", new[] { "Stick up", "Palanca arriba", "Steuerknüppel hoch", "Manche para cima" } },
            { "Manche vers le bas", new[] { "Stick down", "Palanca abajo", "Steuerknüppel runter", "Manche para baixo" } },
            { "Manche (haut / bas)", new[] { "Stick (up / down)", "Palanca (arriba / abajo)", "Steuerknüppel (hoch / runter)", "Manche (cima / baixo)" } },
            { "Accélérer (postcombustion)", new[] { "Accelerate (afterburner)", "Acelerar (postcombustión)", "Beschleunigen (Nachbrenner)", "Acelerar (pós-combustão)" } },
            { "Ralentir", new[] { "Slow down", "Frenar", "Abbremsen", "Desacelerar" } },
            { "Mitrailleuse (Vulcan)", new[] { "Machine gun (Vulcan)", "Ametralladora (Vulcan)", "Maschinengewehr (Vulcan)", "Metralhadora (Vulcan)" } },
            { "Missiles", new[] { "Missiles", "Misiles", "Raketen", "Mísseis" } },
            { "Leurres (Flares)", new[] { "Flares", "Bengalas (Flares)", "Täuschkörper (Flares)", "Iscas (Flares)" } },
            { "Esquive (Evade)", new[] { "Evade", "Esquivar (Evade)", "Ausweichen (Evade)", "Esquiva (Evade)" } },
            { "Start / Pause", new[] { "Start / Pause", "Start / Pausa", "Start / Pause", "Start / Pausa" } },
            { "Menu : valider", new[] { "Menu: confirm", "Menú: aceptar", "Menü: bestätigen", "Menu: confirmar" } },
            { "Menu : retour", new[] { "Menu: back", "Menú: atrás", "Menü: zurück", "Menu: voltar" } },
            { "Menu : haut", new[] { "Menu: up", "Menú: arriba", "Menü: hoch", "Menu: cima" } },
            { "Menu : bas", new[] { "Menu: down", "Menú: abajo", "Menü: runter", "Menu: baixo" } },
            { "Menu : gauche", new[] { "Menu: left", "Menú: izquierda", "Menü: links", "Menu: esquerda" } },
            { "Menu : droite", new[] { "Menu: right", "Menú: derecha", "Menü: rechts", "Menu: direita" } },
            { "Takeover 2 (inutilisé) : ", new[] { "Takeover 2 (unused): ", "Takeover 2 (sin uso): ", "Takeover 2 (unbenutzt): ", "Takeover 2 (não usado): " } },
            { "Disposition", new[] { "Layout", "Disposición", "Anordnung", "Disposição" } },
            { "Garder tout le HUD à l'écran (écrans larges)", new[] { "Keep the whole HUD on screen (wide screens)", "Mantener todo el HUD en pantalla (pantallas anchas)", "Ganzes HUD im Bild halten (Breitbild)", "Manter todo o HUD na tela (telas largas)" } },
            { "Le HUD d'origine est prévu pour le 4:3 : en 16:9 et 21:9, le haut et le bas sortent de l'écran. Cette option le ramène dans l'image.", new[] { "The original HUD is made for 4:3: in 16:9 and 21:9 its top and bottom leave the screen. This option brings it back into the picture.", "El HUD original está pensado para 4:3: en 16:9 y 21:9 la parte superior e inferior salen de la pantalla. Esta opción lo devuelve a la imagen.", "Das Original-HUD ist für 4:3 gedacht: in 16:9 und 21:9 ragen oben und unten aus dem Bild. Diese Option holt es zurück ins Bild.", "O HUD original foi feito para 4:3: em 16:9 e 21:9 o topo e a base saem da tela. Esta opção o traz de volta para a imagem." } },
            { "Écartement à 1 : blocs plaqués contre les bords de l'image (avec le bezel, contre son écran).", new[] { "Spread at 1: blocks pushed against the edges of the picture (with the bezel, against its screen).", "Separación en 1: bloques pegados a los bordes de la imagen (con el bezel, a su pantalla).", "Abstand 1: Blöcke an den Bildrändern (mit Bezel an dessen Bildschirm).", "Afastamento em 1: blocos encostados nas bordas da imagem (com o bezel, na tela dele)." } },
            { "Jets abattus", new[] { "Jets shot down", "Cazas derribados", "Abgeschossene Jets", "Jatos abatidos" } },
            { "Frégates coulées", new[] { "Frigates sunk", "Fragatas hundidas", "Versenkte Fregatten", "Fragatas afundadas" } },
            { "Vies", new[] { "Lives", "Vidas", "Leben", "Vidas" } },
            { "Altitude", new[] { "Altitude", "Altitud", "Höhe", "Altitude" } },
            { "Aide des boutons", new[] { "Button guide", "Guía de botones", "Tastenhilfe", "Guia de botões" } },
            { "Viseur", new[] { "Crosshair", "Mira", "Fadenkreuz", "Mira" } },
            { "Messages (missile, demi-tour…)", new[] { "Messages (missile, turn back…)", "Mensajes (misil, media vuelta…)", "Meldungen (Rakete, umkehren…)", "Mensagens (míssil, meia-volta…)" } },
            { "Course : position", new[] { "Race: position", "Carrera: posición", "Rennen: Position", "Corrida: posição" } },
            { "Course : tours", new[] { "Race: laps", "Carrera: vueltas", "Rennen: Runden", "Corrida: voltas" } },
            { "Course : temps au tour", new[] { "Race: lap times", "Carrera: tiempos por vuelta", "Rennen: Rundenzeiten", "Corrida: tempos de volta" } },
            { "Instruments de vol (horizon, assiette)", new[] { "Flight instruments (horizon, attitude)", "Instrumentos de vuelo (horizonte, actitud)", "Fluginstrumente (Horizont, Fluglage)", "Instrumentos de voo (horizonte, atitude)" } },
            { "Cadres de visée sur les ennemis", new[] { "Target frames on enemies", "Marcos de objetivo sobre los enemigos", "Zielrahmen auf Gegnern", "Molduras de mira nos inimigos" } },
        };
    }
}
