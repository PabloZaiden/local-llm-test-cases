# Local LLM test cases

This is a tool used to evaluate responses from local LLMs while invoking OpenAI-style tools.

## Usage

- Configure the OpenAI-compatible endpoint for the local LLM in the `appsettings.json` file.

```json
{
    "Endpoint": "http://localhost:1234"
}
```

- In the `prompts` directory, add `.txt` files starting with `{number}-` to represent the test prompts.

- To specify a system prompt, add a file named `system.txt` in the `prompts` directory.

- Run the tool using the following command:

```bash
dotnet run
```
