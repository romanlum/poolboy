# Development instructions

Nanoframework has a tight coupled dependecy system, so the firmware and the packages have to match.

To use this project you have to flash your esp32 with firmware `1.7.4`

```
nanoff.exe--update --platform esp32 --serialport COM5  --fwversion 1.7.4.0 --baud 1500000
```