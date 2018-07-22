using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MathNet.Numerics.LinearAlgebra;

public class GeneticAlgorithm : MonoBehaviour {
	[HideInInspector]
	public Chromosome[] Population;
	
	// Matrix builder object.
	MatrixBuilder<float> B = Matrix<float>.Build;

	int populationSize;

	int Layer1UnitCount = 0;
	int Layer2UnitCount = 0;
	int Layer3UnitCount = 0;

	// Create all chromosomes and store them in array.
	public void createPopulation(int popSize, int Layer1UnitCount, int Layer2UnitCount, int Layer3UnitCount){
		populationSize = popSize;
		Population = new Chromosome[populationSize];

		this.Layer1UnitCount = Layer1UnitCount;
		this.Layer2UnitCount = Layer2UnitCount;
		this.Layer3UnitCount = Layer3UnitCount;

		for(int p=0; p<populationSize; p++){
			Population[p] = new Chromosome(Layer1UnitCount, Layer2UnitCount, Layer3UnitCount);
		}
	}

	public void nextGeneration(){
		int newIndividualCount = populationSize / 2;

		for(int newInd=0; newInd<newIndividualCount; newInd++){
			//////////////////////
			// SELECTION
			//////////////////////

			Chromosome Parent1 = null;
			Chromosome Parent2 = null;
			Chromosome Child = null;

			Matrix<float> theta1 = B.Dense(Layer2UnitCount, Layer1UnitCount);
			Matrix<float> theta2 = B.Dense(Layer3UnitCount, Layer2UnitCount);

			int tournamentSize = populationSize;

			// Tournament Selection
			int maxFitness = -1;
			do{
				// Parent 1
				maxFitness = -1;
				for(int t=0; t<tournamentSize; t++){
					int randomC = (int) UnityEngine.Random.Range(0, populationSize-1);
					if(Population[randomC].fitness > maxFitness){
						Parent1 = Population[randomC];
						maxFitness = Population[randomC].fitness;
					}
				}
				// Parent 2
				maxFitness = -1;
				for(int t=0; t<tournamentSize; t++){
					int randomC = (int) UnityEngine.Random.Range(0, populationSize-1);
					if(Population[randomC].fitness > maxFitness){
						Parent2 = Population[randomC];
						maxFitness = Population[randomC].fitness;
					}
				}
			// If parents are same, repeat the process above.
			}while(Parent1 == Parent2 || Parent1 == null || Parent2 == null);

			// Best Selection
			// int maxFitness = -1;
			// do{
			// 	// Parent 1
			// 	maxFitness = -1;
			// 	for(int p=0; p<populationSize; p++){
			// 		if(Population[p].fitness > maxFitness){
			// 			Parent1 = Population[p];
			// 			maxFitness = Population[p].fitness;
			// 		}
			// 	}
			// 	// Parent 2
			// 	maxFitness = -1;
			// 	for(int p=0; p<populationSize; p++){
			// 		if(Population[p].fitness > maxFitness && Population[p] != Parent1){
			// 			Parent2 = Population[p];
			// 			maxFitness = Population[p].fitness;
			// 		}
			// 	}
			// // If parents are same, repeat the process above.
			// }while(Parent1 == Parent2 || Parent1 == null || Parent2 == null);

			//////////////////////
			// CROSSOVER
			//////////////////////

			// Uniform Crossover (with each weight)
			// // Theta1
			// for(int i=0; i<theta1.RowCount; i++){
			// 	for(int j=0; j<theta1.ColumnCount; j++){
			// 		if(UnityEngine.Random.value >= 0.5f){
			// 			theta1.At(i, j, Parent1.Theta1.At(i, j));
			// 		}else{
			// 			theta1.At(i, j, Parent2.Theta1.At(i, j));
			// 		}
			// 	}
			// }
			// // Theta2
			// for(int i=0; i<theta2.RowCount; i++){
			// 	for(int j=0; j<theta2.ColumnCount; j++){
			// 		if(UnityEngine.Random.value >= 0.5f){
			// 			theta2.At(i, j, Parent1.Theta2.At(i, j));
			// 		}else{
			// 			theta2.At(i, j, Parent2.Theta2.At(i, j));
			// 		}
			// 	}
			// }

			// Uniform Crossover (with each neuron's weight)
			// Theta1
			for(int i=0; i<theta1.RowCount; i++){
				if(UnityEngine.Random.value >= 0.5f){
					for(int j=0; j<theta1.ColumnCount; j++){
						theta1.At(i, j, Parent1.Theta1.At(i, j));
					}
				}else{
					for(int j=0; j<theta1.ColumnCount; j++){
						theta1.At(i, j, Parent2.Theta1.At(i, j));
					}
				}
			}
			// Theta2
			for(int i=0; i<theta2.RowCount; i++){
				if(UnityEngine.Random.value >= 0.5f){
					for(int j=0; j<theta2.ColumnCount; j++){
						theta2.At(i, j, Parent1.Theta2.At(i, j));
					}
				}else{
					for(int j=0; j<theta2.ColumnCount; j++){
						theta2.At(i, j, Parent2.Theta2.At(i, j));
					}
				}
			}

			//////////////////////
			// MUTATION
			//////////////////////
			float mutationRate = 0.05f;

			// Swap Mutation (with each weight)
			// // Theta1
			// for(int i=0; i<theta1.RowCount; i++){
			// 	for(int j=0; j<theta1.ColumnCount; j++){
			// 		if(UnityEngine.Random.value < mutationRate){
			// 			int ri = (int) UnityEngine.Random.Range(0, theta1.RowCount-1);
			// 			int rj = (int) UnityEngine.Random.Range(0, theta1.ColumnCount-1);
			// 			theta1.At(i, j, theta1.At(ri, rj));
			// 		}
			// 	}
			// }
			// // Theta2
			// for(int i=0; i<theta2.RowCount; i++){
			// 	for(int j=0; j<theta2.ColumnCount; j++){
			// 		if(UnityEngine.Random.value < mutationRate){
			// 			int ri = (int) UnityEngine.Random.Range(0, theta2.RowCount-1);
			// 			int rj = (int) UnityEngine.Random.Range(0, theta2.ColumnCount-1);
			// 			theta2.At(i, j, theta2.At(ri, rj));
			// 		}
			// 	}
			// }

			// Swap Mutation (with each neuron's weight)
			// Theta1
			for(int i=0; i<theta1.RowCount; i++){
				if(UnityEngine.Random.value < mutationRate){
					int ri = (int) UnityEngine.Random.Range(0, theta1.RowCount-1);
					for(int j=0; j<theta1.ColumnCount; j++){
						theta1.At(i, j, theta1.At(ri, j));
					}
				}
			}
			// Theta2
			for(int i=0; i<theta2.RowCount; i++){
				if(UnityEngine.Random.value < mutationRate){
					int ri = (int) UnityEngine.Random.Range(0, theta2.RowCount-1);
					for(int j=0; j<theta2.ColumnCount; j++){
						theta2.At(i, j, theta2.At(ri, j));
					}
				}
			}

			//////////////////////
			// SURVIVOR SELECTION
			//////////////////////
			// Tournament Selection
			int minFitness = 999999;	// This value should be high as possible.
			int loserIndex = 0;
			for(int t=0; t<tournamentSize; t++){
				int randomC = (int) UnityEngine.Random.Range(0, populationSize-1);
				if(Population[randomC].fitness < minFitness && Population[randomC].fitness != -1){
					loserIndex = randomC;
					minFitness = Population[randomC].fitness;
				}
			}

			// Worst Selection
			// int minFitness = 999999;	// This value should be high as possible.
			// int loserIndex = 0;
			// for(int p=0; p<populationSize; p++){
			// 	if(Population[p].fitness < minFitness && Population[p].fitness != -1){
			// 		loserIndex = p;
			// 		minFitness = Population[p].fitness;
			// 	}
			// }

			// Create child chromosome, and put it to population.
			Child = new Chromosome(theta1, theta2);
			Population[loserIndex] = Child;
		}
	}
}

public class Chromosome{
	// Matrix builder object.
	MatrixBuilder<float> B = Matrix<float>.Build;

	public Matrix<float> Theta1;
	public Matrix<float> Theta2;
	public int fitness;

	public Chromosome(int Layer1UnitCount, int Layer2UnitCount, int Layer3UnitCount){
		// Define weights as random.
		Theta1 = B.Random(Layer2UnitCount, Layer1UnitCount);
		Theta2 = B.Random(Layer3UnitCount, Layer2UnitCount);
		fitness = 0;
	}

	public Chromosome(Matrix<float> theta1, Matrix<float> theta2){
		Theta1 = theta1;
		Theta2 = theta2;
		fitness = -1;
	}
}