using DVRP.Application.Abstractions;
using DVRP.Application.Helpers;
using DVRP.Domain.Entities;

namespace DVRP.Application.Handlers;

public class GeneticAlgorithmSolver : IDvrpSolver
{
    public DvrpSolution Solve(DvrpModel model, DvrpSolverParameters parameters, DvrpSolution? initialSolution = null)
    {
        if(parameters is not GeneticAlgorithmParameters gaParameters)
        {
            throw new ArgumentException("Not suitable parameter", nameof(parameters));
        }

        // 1. Initialize the population
        List<DvrpSolution> population = InitializePopulation(model, gaParameters);

        // 2. Evaluate the initial population
        EvaluatePopulation(model, population);

        // 3. Main loop
        for (int generation = 0; generation < gaParameters.MaxGenerations; generation++)
        {
            // 3.1 Selection
            var selectedParents = SelectionMethods.RouletteWheelSelection(population, population.Count);

            // 3.2 Crossover
            var offspring = PerformCrossover(model, selectedParents, gaParameters);

            // 3.3 Mutation
            PerformMutation(offspring, gaParameters);

            // 3.4 Replacement
            ReplacePopulation(population, offspring);

            // 3.5 Evaluate the population
            EvaluatePopulation(model, population);
        }

        // 4. Get the best solution from the final population
        var bestSolution = GetBestSolution(population);
        return bestSolution;
    }

    private static List<DvrpSolution> InitializePopulation(DvrpModel model, GeneticAlgorithmParameters gaParameters)
    {
        List<DvrpSolution> population = new(gaParameters.PopulationSize);

        for (int i = 0; i < gaParameters.PopulationSize; i++)
        {
            var solution = InitializationMethods.HybridInitialization(model, 0.3);
            population.Add(solution);
        }

        return population;
    }

    private static void EvaluatePopulation(DvrpModel model, List<DvrpSolution> population)
    {
        foreach (DvrpSolution solution in population)
        {
            solution.CalculateFitness(model.Depots.Count);
        }
    }

    private static List<DvrpSolution> PerformCrossover(DvrpModel model, List<DvrpSolution> selectedParents, GeneticAlgorithmParameters gaParameters)
    {
        List<DvrpSolution> offspringPopulation = new(selectedParents.Count);

        for (int i = 0; i < selectedParents.Count; i += 2)
        {
            DvrpSolution parent1 = selectedParents[i];
            DvrpSolution parent2 = selectedParents[i + 1];

            if (Random.Shared.NextDouble() < gaParameters.CrossoverRate)
            {
                (DvrpSolution offspring1, DvrpSolution offspring2) = CrossoverMethods.OrderCrossover(model, parent1, parent2);
                offspringPopulation.Add(offspring1);
                offspringPopulation.Add(offspring2);
            }
            else
            {
                offspringPopulation.Add(parent1);
                offspringPopulation.Add(parent2);
            }
        }

        return offspringPopulation;
    }

    private static void PerformMutation(List<DvrpSolution> offspring, GeneticAlgorithmParameters gaParameters)
    {
        foreach (DvrpSolution solution in offspring)
        {
            MutationMethods.InverseMutation(solution, gaParameters.MutationRate);
        }
    }

    private static void ReplacePopulation(List<DvrpSolution> population, List<DvrpSolution> offspring)
    {
        int elitismSize = (int)(population.Count * 0.1); // Select the top 10% of the solutions

        // Select the best solutions from the current population
        List<DvrpSolution> eliteSolutions = population
            .OrderBy(solution => solution.Fitness)
            .Take(elitismSize)
            .ToList();

        // Remove the worst solutions from the offspring
        offspring = offspring
            .OrderByDescending(solution => solution.Fitness)
            .Skip(elitismSize)
            .ToList();

        // Add the elite solutions to the offspring
        offspring.AddRange(eliteSolutions);

        // Replace the current population with the new offspring solutions
        population.Clear();
        population.AddRange(offspring);
    }

    private static DvrpSolution GetBestSolution(List<DvrpSolution> population)
    {
        // Find the solution with the lowest fitness value
        DvrpSolution bestSolution = population
            .OrderBy(solution => solution.Fitness)
            .First();

        return bestSolution;
    }
}
