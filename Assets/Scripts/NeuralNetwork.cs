using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using MathNet.Numerics.LinearAlgebra;

public class NeuralNetwork : MonoBehaviour {
	// Matrix builder object.
	MatrixBuilder<float> B = Matrix<float>.Build;

	// Sigmoid activation function applied on network.
	Matrix<float> Sigmoid(Matrix<float> x){
		Matrix<float> xx = B.Dense(x.RowCount, x.ColumnCount);

		for(int i=0; i<x.RowCount; i++){
			for(int j=0; j<x.ColumnCount; j++){
				float tmpSig = (float) (1f / (1 + Math.Exp(-x.At(i, j))));
				xx.At(i, j, tmpSig);
			}
		}

		return xx;
	}

	// Softmax layer.
	Matrix<float> Softmax(Matrix<float> x){
		Matrix<float> xx = B.Dense(x.RowCount, x.ColumnCount);

		float expSum = 0f;

		for(int i=0; i<x.RowCount; i++){
			for(int j=0; j<x.ColumnCount; j++){
				x.At(i, j, (float) Math.Exp(x.At(i, j)));

				expSum += x.At(i, j);
			}
		}

		for(int i=0; i<x.RowCount; i++){
			for(int j=0; j<x.ColumnCount; j++){
				xx.At(i, j, (float) (x.At(i, j) / expSum));
			}
		}

		return xx;
	}

	// Feed forwards with given input and weights.
	public Matrix<float> feedForward(Matrix<float> x, Matrix<float> Theta1, Matrix<float> Theta2){
		Matrix<float> a1 = Sigmoid(Theta1 * x);
		Matrix<float> a2 = Sigmoid(Theta2 * a1);

		// Softmax Activation in final layer.
		// a2 = Softmax(a2);

		return a2;
	}
}