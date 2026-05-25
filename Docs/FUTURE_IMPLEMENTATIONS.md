# Future Implementations - MachineNet VR
## Logged May 25, 2026 - Buenos Aires

### #28 - Pop Atom Math Game
- Timer ticking, atoms light up, slap the correct one
- Speed rounds, leaderboards. $10 Quest game.

### #29 - VR Sticker Keyboard (Reflective Dot Tracking)
- Small colored dot stickers on fingertips + table surface markers
- Quest hand tracking already maps all 10 fingers
- Stickers provide overcorrection/calibration anchors
- Table dots anchor virtual keyboard position
- Hardware: 6000 pegatinas 1/4 pulgada MercadoLibre
- Docs needed: 3 PDFs (Quest hand API, OpenXR joints, finger occlusion) + KEYBOARD_TRACKING.md

### #30 - Haptic Math Touch  
- Controller vibration intensity = face residual (blue=calm, red=buzz)
- Pentagon vs hexagon = different haptic signature
- LaTeX formula floats above touched face
- Sound per face (GKAudio.cs framework exists)

### #31 - VR Sandbox Builder
- C60 at chest height, pinch to refine, spread to zoom
- BONEWORKS-style: topology grows around observer
- Age of Empires god-view + Fruit Ninja slap

## Archive
- APKs: C:\MnetUni\Mnet\MachineNet_v*.apk (7 builds)
- VR Vids: ...\MNetv1\logs\VsimsTestsVid\ (4 recordings, 255 MB total)
- v5.1 = PROVEN: 36 FPS, no crash, all colors, hand tracking works
