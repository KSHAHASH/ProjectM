# Budget Planner Frontend

This project was generated with [Angular CLI](https://github.com/angular/angular-cli) version 10.x.

## Project Structure

The application follows a feature-based architecture:

```
src/app/
├── core/              # Singleton services and core functionality
│   ├── guards/        # Route guards
│   ├── interceptors/  # HTTP interceptors
│   ├── models/        # Core domain models
│   └── services/      # Core services (auth, api, etc.)
├── shared/            # Shared components, directives, and pipes
│   ├── components/    # Reusable components
│   ├── directives/    # Shared directives
│   ├── pipes/         # Shared pipes
│   └── modules/       # Shared modules
└── features/          # Feature modules
```

## Development server

Run `ng serve` for a dev server. Navigate to `http://localhost:4200/`. The app will automatically reload if you change any of the source files.

## Build

Run `ng build` to build the project. The build artifacts will be stored in the `dist/` directory. Use the `--prod` flag for a production build.

## Running tests

Run `ng test` to execute the unit tests via [Karma](https://karma-runner.github.io).

Run `ng e2e` to execute the end-to-end tests via [Protractor](http://www.protractortest.org/).
