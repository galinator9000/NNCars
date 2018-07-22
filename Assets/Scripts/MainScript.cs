using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MathNet.Numerics.LinearAlgebra;

public class MainScript : MonoBehaviour {
	private int Layer1UnitCount = 4;
	private int Layer2UnitCount = 15;
	private int Layer3UnitCount = 3;
	
	public GameObject carPrefab;
	public GameObject mainCamera;
	public GameObject CheckpointsGO;
	
	int populationSize = 200;
	float secPerGeneration = 60f;

	Vector3 cameraVec;
	bool followCameraMiddle = false;

	// Matrix builder object.
	MatrixBuilder<float> B = Matrix<float>.Build;

	// Other components.
	NeuralNetwork NN;
	GeneticAlgorithm GA;
	int generationCounter = 0;
	int bestFitness = 0;

	// Car variables.
	GameObject[] Cars;
	CarController[] CarsC;

	Matrix<float> inputX;
	Matrix<float> outputY;
	float beginTime;

	void Start(){
		beginTime = Time.time;

		inputX = B.Dense(Layer1UnitCount, 1);

		// Create population with specific size.
		NN = gameObject.GetComponent<NeuralNetwork>();
		GA = gameObject.GetComponent<GeneticAlgorithm>();
		GA.createPopulation(populationSize, Layer1UnitCount, Layer2UnitCount, Layer3UnitCount);

		CheckpointsGO = Instantiate(CheckpointsGO);

		// Create car objects from prefab and store them in array.
		Cars = new GameObject[populationSize];
		CarsC = new CarController[populationSize];
		for(int c=0; c<populationSize; c++){
			Cars[c] = Instantiate(carPrefab);
			CarsC[c] = Cars[c].gameObject.GetComponent<CarController>();

			CarsC[c].CheckpointsGO = CheckpointsGO;
		}

		// Cars should not collide each other!
		for(int c=0; c<populationSize; c++){
			for(int cc=0; cc<populationSize; cc++){
				if(c != cc){
					Physics.IgnoreCollision(Cars[c].GetComponent<Collider>(), Cars[cc].GetComponent<Collider>());
				}
			}
		}

		// Calculate camera position and vector to look at middle of all cars.
		cameraVec = mainCamera.transform.position;
		cameraVec = cameraVec - CarsC[0].transform.position;
	}

	void Update(){
		// Take all cars' state, feed to Neural Network with their weights finally set speed and steering depend on output.
		for(int c=0; c<populationSize; c++){
			if(!CarsC[c].Dead){
				inputX.At(0, 0, CarsC[c].distances[0]);
				inputX.At(1, 0, CarsC[c].distances[1]);
				inputX.At(2, 0, CarsC[c].distances[2]);
				inputX.At(3, 0, CarsC[c].Speed);

				outputY = NN.feedForward(inputX, GA.Population[c].Theta1, GA.Population[c].Theta2);

				CarsC[c].Speed = outputY.At(0, 0);
				CarsC[c].steerLeft = outputY.At(1, 0);
				CarsC[c].steerRight = outputY.At(2, 0);
			}
		}

		int aliveCarCount = 0;
		// Camera mode 1, follow middle of the alive cars.
		if(followCameraMiddle){
			Vector3 middleOfCars = new Vector3(0f, 0f, 0f);
			for(int c=0; c<populationSize; c++){
				if(!CarsC[c].Dead){
					middleOfCars = middleOfCars + Cars[c].transform.position;
					aliveCarCount = aliveCarCount + 1;
				}
			}

			if(aliveCarCount > 0){
				middleOfCars = middleOfCars / aliveCarCount;
				mainCamera.transform.position = middleOfCars + cameraVec;
			}
		}

		// Camera mode 2, follow the best alive car.
		if(!followCameraMiddle){
			Vector3 bestCarPos;
			int bestCarIndex = 0;
			int bestCarScore = 0;

			for(int c=0; c<populationSize; c++){
				if(!CarsC[c].Dead){
					if(CarsC[c].score > bestCarScore){
						bestCarScore = CarsC[c].score;
						bestCarIndex = c;
					}

					aliveCarCount = aliveCarCount + 1;
				}
			}

			bestCarPos = Cars[bestCarIndex].transform.position;
			mainCamera.transform.position = bestCarPos + cameraVec;
		}

		if(Input.GetKeyDown("c")){
			if(followCameraMiddle){
				print("Following the best alive car.");
				followCameraMiddle = false;
			}else{
				print("Following middle of the alive cars.");
				followCameraMiddle = true;
			}
		}

		// If space pressed, there isn't any car alive or time is just passed, we should skip to the next generation.
		if(Input.GetKeyDown("space") || aliveCarCount <= 0 || ((Time.time - beginTime) >= secPerGeneration)){
			nextGeneration();
		}
	}

	void nextGeneration(){
		beginTime = Time.time;

		generationCounter += 1;
		bestFitness = 0;

		for(int c=0; c<populationSize; c++){
			GA.Population[c].fitness = CarsC[c].score;

			if(CarsC[c].score > bestFitness){
				bestFitness = CarsC[c].score;
			}
			
			CarsC[c].resetCar();
		}

		GA.nextGeneration();
		print("Generation: " + generationCounter.ToString() + " | Best fitness: " + bestFitness.ToString());
	}
}
