# Useful Prompts for AI Agents

- Explain what this service is responsible for, identify the key dependencies, and point out which parts are business logic versus infrastructure concerns. Then suggest a safe first refactor that does not change behavior.

- Create unit tests for this method using the same test style as this project. Cover discount boundaries, null input handling, and the case where the total is capped. Explain any edge cases you think are easy to miss.

- Review this controller action and propose a refactor that moves orchestration into a service without changing the HTTP contract. Show the target shape of the controller, service interface, and unit tests I should expect to update.

- I am adding a new optional region filter to this endpoint. Update the ASP.NET Core handler, adjust the OpenAPI description, and point out any config, docs, or client code that may also need to change.

- Explain this build failure in plain English, tell me which project likely introduced it, and suggest the next two commands I should run to narrow it down.

- This xUnit test is failing intermittently. Based on the output and the file paths involved, what are the likely causes, and what should I inspect first?

- Refactor this background worker to make the retry policy easier to test. Keep the public behavior the same, preserve structured logging, and show me the test cases I should add.

- Add missing unit tests for the CreateOrder flow. Cover validation failures, duplicate order detection, and the downstream payment timeout path. Keep the existing test style, do not rename public APIs, and stop once the new tests pass.

- Update the notification handlers in this project to use the shared Result<T> pattern instead of throwing validation exceptions. Preserve current behavior, update the affected unit tests, and summarize which handlers changed.

- Investigate why dotnet test is failing in the Notifications.Tests project, make the smallest fix that addresses the root cause, rerun the relevant tests, and summarize the change.

- Add correlation ID propagation to the API and background worker pipeline. Update middleware, logging enrichment, and the integration tests that assert the header flows through. Do not change unrelated logging format, and note any follow-up work if you find gaps outside this slice.

- Add support for a new beta environment flag. Update the .NET configuration binding, the Bicep template, the GitHub Actions workflow, and the deployment documentation. Keep naming consistent with the existing environment settings.

