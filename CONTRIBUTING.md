# Contributing to AI Foundry Agent

Thank you for your interest in contributing to the AI Foundry Agent sample application! This document provides guidelines for contributing to this project.

## Getting Started

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes
4. Add tests if applicable
5. Ensure all tests pass (`dotnet test`)
6. Commit your changes (`git commit -m 'Add amazing feature'`)
7. Push to the branch (`git push origin feature/amazing-feature`)
8. Open a Pull Request

## Development Setup

1. Install [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
2. Clone your fork: `git clone https://github.com/YOUR-USERNAME/ai-foundry-agent-basic.git`
3. Navigate to the src directory: `cd ai-foundry-agent-basic/src`
4. Build the project: `dotnet build`
5. Run tests: `dotnet test`

## Project Structure

- `/src` - Main application code
- `/test` - Unit and integration tests
- `/docs` - Documentation (if exists)
- `.github/workflows` - GitHub Actions workflows

## Code Guidelines

- Follow .NET coding conventions
- Use meaningful variable and method names
- Add comments for complex logic
- Maintain consistency with existing code
- Use global using statements (in GlobalUsings.cs) rather than individual using statements in each file
- Write unit tests for new functionality

## Testing

- Write unit tests for all business logic
- Ensure all tests pass before submitting a PR
- Add integration tests when needed

## Pull Request Process

1. Update the README.md with details of changes if needed
2. Add tests for new functionality
3. Ensure all tests pass
4. Update documentation as needed
5. Submit your pull request

## Questions?

If you have questions, please open an issue in the repository.