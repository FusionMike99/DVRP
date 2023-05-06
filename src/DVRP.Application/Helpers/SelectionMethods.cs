using DVRP.Domain.Entities;

namespace DVRP.Application.Helpers;

public static class SelectionMethods
{
    public static List<DvrpSolution> RouletteWheelSelection(List<DvrpSolution> population, int numberOfSelections)
    {
        var selectedSolutions = new List<DvrpSolution>();

        // Invert the fitness values (because it's a minimization problem)
        double totalInvertedFitness = population.Sum(s => 1 / s.Fitness);
        var selectionProbabilities = population.Select(s => (1 / s.Fitness) / totalInvertedFitness).ToList();

        // Calculate cumulative probabilities
        var cumulativeProbabilities = new List<double>();
        double cumulativeProbability = 0;
        foreach (double probability in selectionProbabilities)
        {
            cumulativeProbability += probability;
            cumulativeProbabilities.Add(cumulativeProbability);
        }

        // Perform Roulette Wheel Selection
        for (int i = 0; i < numberOfSelections; i++)
        {
            double randomValue = Random.Shared.NextDouble();
            int selectedIndex = cumulativeProbabilities.IndexOf(cumulativeProbabilities.First(p => p >= randomValue));
            selectedSolutions.Add(population[selectedIndex]);
        }

        return selectedSolutions;
    }

    public static List<DvrpSolution> TournamentSelection(List<DvrpSolution> population, int numberOfSelections, int tournamentSize)
    {
        var selectedSolutions = new List<DvrpSolution>();

        for (int i = 0; i < numberOfSelections; i++)
        {
            var tournamentContestants = new List<DvrpSolution>();

            for (int j = 0; j < tournamentSize; j++)
            {
                int randomIndex = Random.Shared.Next(population.Count);
                tournamentContestants.Add(population[randomIndex]);
            }

            var bestSolution = tournamentContestants.OrderBy(s => s.Fitness).First();
            selectedSolutions.Add(bestSolution);
        }

        return selectedSolutions;
    }

    public static List<DvrpSolution> RankSelection(List<DvrpSolution> population, int numberOfSelections)
    {
        var selectedSolutions = new List<DvrpSolution>();

        // Sort the population by fitness (ascending) and assign ranks
        var rankedPopulation = population.OrderBy(s => s.Fitness).Select((solution, index) => new { solution, rank = index + 1 }).ToList();

        // Calculate the total rank sum
        int totalRankSum = rankedPopulation.Sum(r => r.rank);

        // Calculate the selection probabilities based on the ranks
        var selectionProbabilities = rankedPopulation.Select(r => (double)r.rank / totalRankSum).ToList();

        // Calculate cumulative probabilities
        var cumulativeProbabilities = new List<double>();
        double cumulativeProbability = 0;
        foreach (double probability in selectionProbabilities)
        {
            cumulativeProbability += probability;
            cumulativeProbabilities.Add(cumulativeProbability);
        }

        // Perform Rank Selection
        for (int i = 0; i < numberOfSelections; i++)
        {
            double randomValue = Random.Shared.NextDouble();
            int selectedIndex = cumulativeProbabilities.IndexOf(cumulativeProbabilities.First(p => p >= randomValue));
            selectedSolutions.Add(rankedPopulation[selectedIndex].solution);
        }

        return selectedSolutions;
    }

    public static List<DvrpSolution> TruncationSelection(List<DvrpSolution> population, double truncationThreshold)
    {
        int numberOfSelections = (int)(truncationThreshold * population.Count);

        // Sort the population by fitness (ascending) and select the top individuals
        var selectedSolutions = population.OrderBy(s => s.Fitness).Take(numberOfSelections).ToList();

        return selectedSolutions;
    }
}
