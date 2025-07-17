# dct (Dotnet CLI Tool)

A .NET global CLI tool to rapidly generate CQRS and Clean Architecture components such as commands, queries, handlers, classes, and interfaces. Streamline your development workflow and enforce best practices with customizable templates and configuration.

## Features
- Generate CQRS artifacts: commands, queries, handlers
- Create classes and interfaces with a single command
- Initialize default config and templates
- Supports custom templates and configuration
- Fast, scriptable, and easy to use

## Installation
After publishing to NuGet, install globally with:

```sh
dotnet tool install -g dct
```

## Usage

### Initialize configuration and templates
```sh
dct init
```

### Generate an artifact (command, query, handler, class, interface)
```sh
dct create <artifact> <path>
```
- `<artifact>`: Type of artifact to generate (e.g., `command`, `query`, `handler`, `class`, `interface`)
- `<path>`: Target path or name for the generated file(s)

#### Examples
| Command | Description |
|---------|-------------|
| `dct init` | Initialize config and templates |
| `dct create command User/CreateUser` | Generate a command in `User/CreateUser` |
| `dct create handler User/CreateUserHandler` | Generate a handler |
| `dct create query User/GetUser` | Generate a query |
| `dct create class Models/User` | Generate a class |
| `dct create interface Services/IUserService` | Generate an interface |

## Customization

### Configuration
Customize generation using `.dctconfig.json` in your project root. Example:

```json
{
  "namespace": "MyApp.Features",
  "author": "YourName"
}
```

### Templates
You can override default templates by placing your own in the `Templates/` directory. Supported templates:
- `command.scriban`
- `handler.scriban`
- `query.scriban`

Edit these files to match your coding standards and patterns.

## Development

### Dependencies
- [Spectre.Console.Cli](https://www.nuget.org/packages/Spectre.Console.Cli)
- [Scriban](https://www.nuget.org/packages/Scriban)

## License

MIT License

---

© 2024 Mikheil Berishvili. See [LICENSE](LICENSE) for details. 