# API Integration with Semantic Kernel

**Important Note:**  
This project is a **prototype** developed with the primary goal of testing API integration with the Semantic Kernel. It does **not strictly follow best practices** in terms of software architecture and design patterns, as the main focus was on quickly validating core concepts related to AI-driven agents. While fully functional, the code serves as an experimental proof of concept rather than a production-ready solution.

That said, it is a robust demonstration of how to integrate AI functionalities using a .NET 8 API framework, handling real-world scenarios such as user sentiment analysis, functionality detection, and parameter management.

## Project Overview

This API is designed to showcase the potential of integrating AI into enterprise resource planning (ERP) systems. The core aim is to leverage the Semantic Kernel for managing agents that handle various tasks, such as detecting user intentions, managing functional parameters, and even responding to user sentiment.

The project implements a modular structure where each agent is responsible for a specific function, making it scalable and adaptable for future improvements.

### Key Features:
- **Built with .NET 8 and Semantic Kernel**  
- **Modular agent architecture** for scalability and easy maintenance  
- **Integration with ERP** for real-time execution of identified functionalities

## Agents Overview

The project utilizes several specialized agents to handle different aspects of user interaction and functionality detection:

- **Functionality Detection Agent**  
  This agent allows the system to identify desired functionalities through natural language input. By analyzing user requests, it helps ensure that the right features are executed without the need for technical commands.

- **Parameter Handling Agent**  
  Once functionalities are identified, this agent presents the necessary parameters required for the operation, such as user details, dates, or specific values. This ensures that interactions are fluid and transparent, reducing complexity for the end user.

- **Sentiment Analysis Agent**  
  This agent interprets the sentiment behind user inputs (positive, negative, neutral) and adjusts responses accordingly. By understanding the user's tone, it helps create a more natural and user-friendly interaction.

- **Message Service Agent**  
  The message service humanizes system communications, utilizing advanced translation and interpretation techniques to make system messages more user-friendly and understandable. This agent aims to bridge the gap between technical data and human-readable content.

## Architecture

The project follows a modular controller-based architecture, with each agent having its own controller to manage functionality. The architecture was designed with scalability in mind, ensuring that new agents or features can be added with minimal refactoring. The main components include:

1. **Controllers**:  
   Each agent is handled by its dedicated controller, allowing for isolated functionality and better code management.

2. **API Integration**:  
   The project uses .NET 8 Web API for communication between the ERP and the Semantic Kernel, ensuring smooth integration and real-time functionality execution.

3. **Semantic Kernel**:  
   The Semantic Kernel is at the heart of the project, managing agent behaviors, decision-making, and AI-driven responses.

## Installation

To set up this project, follow the steps below:

1. Clone the repository:
    ```bash
    git clone https://github.com/your-repo/your-project.git
    ```

2. Navigate to the project directory:
    ```bash
    cd your-project
    ```

3. Install the necessary dependencies:
    ```bash
    dotnet restore
    ```

4. Build the project:
    ```bash
    dotnet build
    ```

5. Run the API locally:
    ```bash
    dotnet run
    ```

## Usage

Once the API is running, you can interact with the agents via the exposed API endpoints. Each agent has its own controller and endpoints:

- **Functionality Detection**: Detects user intentions and returns the appropriate function.
- **Parameter Handling**: Fills in required parameters for identified functionalities.
- **Sentiment Analysis**: Analyzes user sentiment and adjusts responses accordingly.
- **Message Service**: Humanizes system messages based on user interactions.

You can explore the API endpoints through Swagger, which is included in the project for ease of testing.

## Known Issues

- **Incomplete Parameter Filling**: While the system accurately detects most parameters, there are edge cases where the parameter handling agent may not correctly fill all parameters. Ongoing improvements are focused on refining this functionality.

- **Controller Dependency Communication**: Communication between some controllers still needs optimization, especially regarding dependency injection and data sharing.

## Future Improvements

- **Enhancing Parameter Handling**: Implement a more robust system for managing and validating parameters, possibly integrating caching or external services for better accuracy.
  
- **Advanced AI Models**: Investigate the use of more sophisticated AI models, such as GPT-based services, for deeper natural language understanding and parameter handling.

- **Dedicated AI API**: Separate the AI functionalities into their own API project to avoid conflicts with existing ERP logic and ensure smooth, scalable integration.

## Contributing

While this project was developed for internal testing and experimentation, contributions are welcome. Feel free to submit issues, feature requests, or pull requests. Please ensure that your code follows the general project structure and standards.
