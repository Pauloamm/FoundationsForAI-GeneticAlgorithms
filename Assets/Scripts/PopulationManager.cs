using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Random = System.Random;

public class PopulationManager : MonoBehaviour
{
    public int currentGeneration = 1;


    //MUTATION
    public float mutationProbability = 0.42f;

    struct Parents
    {
        public Movement firstParent;
        public Movement secondParent;
    }

    public int chromossomeSize = 9;

    public GameObject objectToSpawn;
    public int populationNumber = 50;

    private List<Movement> population;


    public int currentPopulationAlive;

    void GiveRandomChromossomes()
    {
        Random rg = new Random();

        for (int i = 0; i < populationNumber; i++)
        {
            Movement newGameObject = Instantiate(objectToSpawn).GetComponent<Movement>();

            float[] newChromossome = new float[chromossomeSize];


            for (int index = 0; index < newChromossome.Length; index++)
            {
                //wiskersWeights[index] = 1;
                newChromossome[index] = (float) rg.NextDouble() * 2f - 1;
            }

            newGameObject.wiskersWeights = newChromossome;
            newGameObject.OnDestroyEvent += UpdateNumberAlive;
            population.Add(newGameObject);
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        currentPopulationAlive = populationNumber;
        population = new List<Movement>();
        GiveRandomChromossomes();
    }

    // Update is called once per frame

    void UpdateNumberAlive()
    {
        currentPopulationAlive--;
        if (currentPopulationAlive == 0)
        {
            CreateNewGeneration();
            currentPopulationAlive = populationNumber;
        }
    }


    void CreateNewGeneration()
    {
        population = population.OrderBy(mov => mov.GetAliveTime).ToList();

        NormalizeAliveTimes();

        List<Parents> selectedParents = SelectionRoulette();
        //List<Parents> selectedParents = TournamentSelection();

        TwoPointCrossover(selectedParents);
    }
    
    void NormalizeAliveTimes()
    {
        float highestAliveTime = 0;
        foreach (Movement individual in population) 
            if (highestAliveTime < individual.GetAliveTime) highestAliveTime = individual.GetAliveTime;
        foreach (Movement individual in population) individual.NormalizeTimeAlive(highestAliveTime);

        population = population.OrderBy(o => o.GetAliveTime).ToList();
    }

    List<Parents> SelectionRoulette()
    {
        List<Parents> selectedPairs = new List<Parents>();


        Parents tempParents = new Parents();

        while (selectedPairs.Count < populationNumber)
        {
            Random rg = new Random();
            float newNumber = (float) rg.NextDouble();

            //float accumulativeProb = 0;

            foreach (Movement individual in population)
            {
                //accumulativeProb += individual.GetAliveTime;

                if (newNumber >= individual.GetAliveTime) continue;

                if (tempParents.firstParent == null)
                {
                    tempParents.firstParent = individual;
                    break;
                }

                if (tempParents.firstParent == individual) break;

                tempParents.secondParent = individual;
                selectedPairs.Add(tempParents);
                tempParents = new Parents();
                break;
            }
        }


        return selectedPairs;
    }

    List<Parents> TournamentSelection()
    {
        List<Parents> selectedParents = new List<Parents>();

        int numberOfTournamentParticipants = 3;

        List<Movement> selectedParticipants = new List<Movement>();

        Parents newParentPair = new Parents();
        do
        {
            List<int> selectedIndexes = new List<int>();
            do
            {
                Random rg = new Random();


                int randomSelectIndex = rg.Next(0, populationNumber);

                bool alreadySelected = false;
                foreach (int index in selectedIndexes)
                {
                    if (index == randomSelectIndex)
                        alreadySelected = true;
                }

                if (!alreadySelected) selectedIndexes.Add(randomSelectIndex);
                
                
            } while (selectedIndexes.Count < numberOfTournamentParticipants);
            
            
            
            Movement tournamentWinner = null;

            foreach (int index in selectedIndexes)
            {
                if (tournamentWinner == null) tournamentWinner = population[index];
                else if (tournamentWinner.GetAliveTime < population[index].GetAliveTime)
                    tournamentWinner = population[index];
            }

            if (newParentPair.firstParent == null) newParentPair.firstParent = tournamentWinner;
            else if(newParentPair.firstParent != tournamentWinner )
            {
                newParentPair.secondParent = tournamentWinner;
                selectedParents.Add(newParentPair);
                newParentPair = new Parents();
            }
        } while (selectedParents.Count < populationNumber);


        return selectedParents;
    }

    void TwoPointCrossover(List<Parents> selectedParents)
    {
        Random rg = new Random();
        int firstPoint = rg.Next(0, chromossomeSize+1);


        int secondPoint;
        do
        {
            secondPoint = rg.Next(0, chromossomeSize+1);
        } while (secondPoint == firstPoint);


        List<Movement> newGeneration = new List<Movement>();


        int counterForName = 1;
        foreach (Parents ps in selectedParents)
        {
            float[] newChromossome = new float[chromossomeSize];
            
            //CHECK FOR REFERENCE/VALUE TYPE
            //newChromossome = ps.firstParent.wiskersWeights;

            for (int i = 0; i < chromossomeSize; i++)
            {
                newChromossome[i] = ps.firstParent.wiskersWeights[i];
            }

            for (int index = 0; index < chromossomeSize; index++)
            {
                if ((index >= firstPoint && index <= secondPoint) ||
                    (index <= firstPoint && index >= secondPoint))
                {
                    newChromossome[index] = ps.secondParent.wiskersWeights[index];
                }
            }

            GameObject newObject = Instantiate(objectToSpawn);
            newObject.name = "CarNr " + counterForName++;


            Movement temp = newObject.GetComponent<Movement>();


            if (rg.NextDouble() <= mutationProbability)
            {
                newChromossome = MutateChromossome(newChromossome);
            }

            temp.wiskersWeights = newChromossome;
            temp.OnDestroyEvent += UpdateNumberAlive;
            newGeneration.Add(temp);
        }

        foreach (Movement m in population) Destroy(m.gameObject);
        population = newGeneration;
        currentGeneration++;
    }
    float[] MutateChromossome(float[] originalChromossome)
    {
        Random rg = new Random();

        int geneToMutate = rg.Next(0, chromossomeSize);
        originalChromossome[geneToMutate] = (float) rg.NextDouble() * 2f - 1;

        return originalChromossome;
    }
}