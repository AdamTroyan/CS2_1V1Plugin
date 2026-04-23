# 1v1Round — CS2 Plugin

A **CounterStrikeSharp** plugin for Counter-Strike 2.

> ⚠️ **This is my very first plugin ever made with CounterStrikeSharp.**  
> It is intended for use on **bot servers only** and is **not ready for online/competitive use**.

---

## What it does

When only one player remains alive on each team, the plugin automatically triggers a **1v1 duel** between them using only a Deagle. At the end of the duel, the winner is announced in chat and each player's original loadout is restored for the next round.

### Features

- Auto-detects when a 1v1 situation arises and starts the duel
- Forces both players to use a Deagle for the duel
- Restores each player's weapons after the round ends
- Admin command (`!css_admin1v1`) to toggle the plugin on/off

---

## Requirements

- [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp)
- CS2 server

---

## Installation

1. Build the project or grab the compiled `.dll`
2. Place it in your `csgo/addons/counterstrikesharp/plugins/` folder
3. Restart the server

---

## Current Limitations

- **Designed for bot servers only** — behavior on online servers is untested and likely broken
- No config file support yet
- No per-player settings

---

## Roadmap

This is version `1.0.0`. Future versions will aim to:

- Make the plugin fully functional for online servers
- Add configuration options
- Improve stability and edge case handling

---

## Notes

This project was built as a learning exercise to get familiar with the CounterStrikeSharp API. Feedback and suggestions are welcome!
