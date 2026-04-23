# 1v1 Deagle Duel Plugin (CS2)

A robust, high-performance **CounterStrikeSharp** plugin for Counter-Strike 2 that automatically triggers a 1v1 Deagle duel when only two players remain alive.

## 🚀 Current Features (v1.1.0)

* **Automatic 1v1 Trigger:** Detects when the round reaches a 1v1 situation and initiates the duel.
* **Intelligent Weapon Management:**
    * **Snapshots:** Saves players' current loadouts.
    * **Cleanup:** Removes all weapons and provides a Desert Eagle.
    * **Restoration:** Automatically restores original loadouts at the next player spawn.
* **Anti-Exploit & Optimization:**
    * **Ground Cleanup:** Periodically removes dropped weapons to maintain server performance.
    * **Anti-Drop:** Prevents players from dropping their Deagle during the duel.
* **Multiplayer Optimized:** * Thread-safe collection handling to prevent server crashes.
    * Efficient `OnTick` logic to ensure minimal CPU impact.
    * Clean, professional Chat UI with formatted spacing for better visibility.

## 🛠 Installation

1.  Ensure you have [CounterStrikeSharp](https://github.com/rofl0l/CounterStrikeSharp) installed on your server.
2.  Download the plugin and place the `1v1Round` folder into:
    `game/csgo/addons/counterstrikesharp/plugins/`
3.  Restart the server or use the `css_plugins load 1v1Round` command.

## ⌨️ Commands

* `css_admin1v1`: Toggles the 1v1 system **ON** or **OFF** (Admin only).

## 🔮 Upcoming Features (v1.2.0 - In Development)

The next version will focus on **Persistent Data Collection & Player Analytics**. Planned updates include:

* **Advanced Statistics:** Tracking total duel wins, losses, and win streaks.
* **Skill Tracking:** Monitoring and displaying Headshot percentages for every duel.
* **Leaderboards:** In-game commands to view top duelists on the server.
