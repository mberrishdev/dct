# dct (Dotnet CLI Tool)

[![NuGet Version](https://img.shields.io/nuget/v/dct.svg?logo=nuget)](https://www.nuget.org/packages/dct)

A .NET global CLI tool to rapidly generate CQRS and Clean Architecture components such as commands, queries, handlers, classes, and interfaces. Streamline your development workflow and enforce best practices with customizable templates and configuration.

## Features
- Generate CQRS artifacts: commands, queries, handlers
- Create classes and interfaces with a single command
- Initialize default config and templates
- Fully customizable via templates and configuration
- Fast, scriptable, and easy to use

## Installation
After publishing to NuGet, install globally with:

```sh
dotnet tool install -g dct
```

Or see the latest version on [NuGet.org](https://www.nuget.org/packages/dct).

## Getting Started

**Before using dct, you must initialize and configure it in your project:**

```sh
dct init
```

This command creates a `.dct` folder in your project root, containing all templates and a `.dct-config` file. All code generation is based on these templates and your configuration. You can customize the templates and `.dct/.dct-config` to fit your project's needs. With proper configuration, dct can generate any artifact or file structure you require.

## Usage

### Show version
```sh
dct --version
```

### Initialize configuration and templates (required before first use)
```sh
dct init
```

### Generate an artifact (command, query, handler, class, interface, or any custom template)
```sh
dct create <artifact> <path>
```
- `<artifact>`: Type of artifact to generate (e.g., `command`, `query`, `handler`, `class`, `interface`, or any custom template name)
- `<path>`: Target path or name for the generated file(s)

#### Examples
| Command | Description |
|---------|-------------|
| `dct --version` | Show the installed dct version |
| `dct init` | Initialize config and templates (must be run first) |
| `dct create command User/CreateUser` | Generate a command in `User/CreateUser` |
| `dct create handler User/CreateUserHandler` | Generate a handler |
| `dct create query User/GetUser` | Generate a query |
| `dct create class Models/User` | Generate a class |
| `dct create interface Services/IUserService` | Generate an interface |

## Customization

### Configuration
Customize generation by editing the `.dct/.dct-config` file in your project root. Example:

```json
{
  "templatePaths": {
    "command": "./dct-templates/command.scriban",
    "query": "./dct-templates/query.scriban",
    "handler": "./dct-templates/handler.scriban"
  }
}
```

### Templates
You can override default templates or add your own by placing them in the `.dct/Templates/` directory. Supported templates include:
- `command.scriban`
- `handler.scriban`
- `query.scriban`
- Any custom template you define

Edit these files to match your coding standards and patterns. The tool will use your configuration and templates to generate code exactly as you need.

## Development

### Dependencies
- [Spectre.Console.Cli](https://www.nuget.org/packages/Spectre.Console.Cli)
- [Scriban](https://www.nuget.org/packages/Scriban)

## Contributing

Contributions are welcome! To contribute:

1. Fork this repository and create your branch from `master`.
2. Make your changes and add tests if applicable.
3. Ensure all tests pass (`dotnet test`).
4. Submit a pull request describing your changes.

For feature requests, bug reports, or questions, please open an issue or visit the [NuGet package page](https://www.nuget.org/packages/dct).

## License

MIT License

---

© 2024 Mikheil Berishvili. See [LICENSE](LICENSE) for details. 