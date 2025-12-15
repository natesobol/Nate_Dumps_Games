# Trivia Whip Web Port Plan

This repository currently holds the original B4i assets and exported question data. The goal is to rebuild the game as a Blazor WebAssembly application with an optional ASP.NET Core backend. This document summarizes the translation map from the iOS project to the new stack and lists the concrete implementation steps.

## Architecture Overview
- **Frontend:** Blazor WebAssembly with Razor components for title, category selection, question flow, score, settings, store, feedback, and (optionally) leaderboard pages.
- **Backend (optional):** ASP.NET Core minimal APIs for profile persistence, email sending, and leaderboards.
- **Data:** Static JSON question files (e.g., `wwwroot/data/questions.json`) generated from `Assets.bas`/`Assets2.bas`. Client-side LocalStorage holds settings and profile data.
- **Services:** C# services for questions, gameplay engine, settings/profile persistence, achievements, audio, timers, purchases, and (optionally) leaderboards/mail.

## Core Data Models
- **Question:** `Id`, `Category`, `SubCategory`, `Prompt`, `Choices`, `AnswerIndex`.
- **Settings:** Theme, timer/achievement modes, question count, ads toggle, scheme color, avatar, mute state, question-button help, view-on-start and alphabetical toggles, selected categories/subcategories, last email.
- **Profile:** Coins, streak/lives, buffs, idol level, correct/incorrect counts, achievements, level and milestones.
- **Buff:** Coin/correct/skip multipliers, optional extra life, identifier.
- **Session:** Active game state including shuffled questions, timers, counts, and lives.
- **AchievementDefinition:** Id, description, predicate to evaluate unlock conditions.

## Page/Component Mapping
- **Title Page:** Displays avatar, coins, level progress, selected categories, and Play/Store/Settings/Leaderboard buttons.
- **Category Page:** Checkbox tree for categories/subcategories plus alphabetical and view-on-start toggles.
- **Question Page:** Shows prompt, shuffled answers, timer, lives, coins, progress bar, skip/50-50, idol reveal, feedback animations, and next navigation.
- **Score Page:** Final stats, coins gained, level progress, unlocked achievements, replay/share/leaderboard actions.
- **Settings Page:** Theme/timer/achievement toggles, question-count slider, scheme color/avatars, mute/help toggles.
- **Store Page:** Purchase scheme colors, avatars, buffs, and idol levels with coins.
- **Feedback Page:** Send feedback/report via backend mail endpoint using stored email address.
- **Tutorial Modals:** Reusable modals for first-time guidance; persist dismissed flags in the profile.
- **Leaderboard Page (optional):** Consumes backend API for score submission and retrieval.

## Service Responsibilities
- **QuestionService:** Load/filter/shuffle questions from JSON, expose lookup and random selection helpers.
- **GameEngine:** Start sessions, shuffle answer order, submit answers, handle timers/timeouts, streaks, lives, coins, milestones, achievements, buffs/idol effects.
- **SettingsService/ProfileService:** Load/save settings and profile to LocalStorage (or backend); raise change events for UI updates.
- **AchievementService:** Registry of achievements and unlock evaluation; trigger toast notifications.
- **AudioService:** Play click/correct/wrong/finish/level-up/idol sounds respecting mute.
- **TimerService:** Manage per-question countdown in timed mode.
- **PurchaseService:** Deduct coins and unlock scheme colors, avatars, buffs, and idol tiers.
- **MailService:** Post feedback/report data to backend for delivery.
- **LeaderboardService (optional):** Simple APIs for submitting and fetching high scores.

## Roadmap
1. Scaffold a hosted Blazor WebAssembly solution and create `wwwroot/data/questions.json` using the existing question export.
2. Implement data models and register all services with dependency injection.
3. Build Razor components for each page, wiring them to services and LocalStorage persistence.
4. Add tutorial modals and toast notifications for achievements/purchases/errors.
5. Implement optional backend endpoints for feedback email and leaderboards.
6. Style the app with consistent scheme colors/avatars and add audio assets; replace iOS-specific visuals with web-friendly equivalents. A unified stylesheet now drives both the Blazor components and the static HTML shell so they share the same compact theme.
7. Test gameplay flows (timed/practice), purchases, achievements, and persistence across sessions.

## Unsupported iOS-Specific Features
Remove or replace iOS-only integrations such as AppTrackingTransparency, Game Center, interstitial ads, and AVAudioSession blocks. Use web equivalents (custom leaderboards, web audio, optional web ads) where desired.

## Repository layout
- `TriviaWhip.Shared/`: Shared models used by both the server and WebAssembly client.
- `TriviaWhip.Server/`: Minimal API backend exposing feedback and leaderboard endpoints plus static file hosting for the client.
- `TriviaWhip.Client/`: Blazor WebAssembly front-end containing Razor components, services, and static assets (questions, styles).
- `questions.json`: Export of the B4i assets copied into the client at `wwwroot/data/questions.json`.

## Running the app
1. Install the .NET 8 SDK locally.
2. Restore and build from the repository root. The server project now references the client, so a server build or publish will bundle the Blazor assets automatically:
   ```bash
   dotnet build TriviaWhip.Server/TriviaWhip.Server.csproj
   dotnet build TriviaWhip.Client/TriviaWhip.Client.csproj
   ```
3. Run the hosted backend (which also serves the client assets):
   ```bash
   dotnet run --project TriviaWhip.Server/TriviaWhip.Server.csproj
   ```
4. Navigate to the served URL (default `https://localhost:5001`) to play Trivia Whip in the browser.

## GitHub Pages deployment (no binary files committed)
- Build artifacts such as the Blazor `_framework` output and the published `wwwroot` directory should **not** be committed to the repository. They are now ignored via `.gitignore`.
- The `.github/workflows/deploy-gh-pages.yml` workflow restores and publishes the client on pushes to the `work` branch, then uploads only the generated `wwwroot` files as a GitHub Pages artifact. This keeps the repository free of binary payloads while still updating the site.
