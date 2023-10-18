{
	"name": "RawScene",
	"type": "Raw",
	"dependencies": [
		{
			"file": "Content/Models/Teapot.obj",
			"type": "Mesh",
			"id": "93250dc6-cba9-40da-b21b-d622bdea6589"
		},
		{
			"file": "Content/Models/Sphere.obj",
			"type": "Mesh",
			"id": "f2ab0a65-d5ac-452f-8b83-f5cf9c91ef5f"
		},
		{
			"file": "Content/Materials/Test.mat",
			"type": "Material",
			"id": "f2578c58-78d5-4822-8286-8b14870ec2f7"
		}
	],
	"components": [
		{
			"id": "af1ade34-7130-44a6-b631-398c0f2487ab",
			"type": "TransformComponent"
		},
		{
			"id": "59ee6f33-19c5-4669-8d78-0bb380d871a3",
			"type": "CameraComponent",
			"FieldOfView": 90,
			"IsMainCamera": true
		},
		{
			"id": "bad71b84-64da-453d-b34c-b56b7d271479",
			"type": "TransformComponent",
			"LocalEulerAngles": {
				"X": 0.0,
				"Y": 0.0,
				"Z": 180.0
			}
		},
		{
			"id": "96845764-dfab-4e59-a45c-6f311b9c335c",
			"type": "MeshRendererComponent",
			"Mesh": "93250dc6-cba9-40da-b21b-d622bdea6589",
			"Material": "f2578c58-78d5-4822-8286-8b14870ec2f7"
		},
		{
			"id": "7ace95bb-aa6b-4d26-bf03-f690e365c104",
			"type": "TransformComponent",
			"Parent": "bad71b84-64da-453d-b34c-b56b7d271479",
			"LocalPosition": {
				"X": 3.0,
				"Y": 0.0,
				"Z": 0.0
			}
		},
		{
			"id": "f1b9c3c9-eb77-48e3-b364-595d68fd9b96",
			"type": "MeshRendererComponent",
			"Mesh": "f2ab0a65-d5ac-452f-8b83-f5cf9c91ef5f",
			"Material": "f2578c58-78d5-4822-8286-8b14870ec2f7"
		}
	],
	"entities": [
		{
			"id": 0,
			"components": [
				{
					"id": "af1ade34-7130-44a6-b631-398c0f2487ab"
				},
				{
					"id": "59ee6f33-19c5-4669-8d78-0bb380d871a3"
				}
			]
		},
		{
			"id": 1,
			"components": [
				{
					"id": "bad71b84-64da-453d-b34c-b56b7d271479"
				},
				{
					"id": "96845764-dfab-4e59-a45c-6f311b9c335c"
				}
			]
		},
		{
			"id": 2,
			"components": [
				{
					"id": "7ace95bb-aa6b-4d26-bf03-f690e365c104"
				},
				{
					"id": "f1b9c3c9-eb77-48e3-b364-595d68fd9b96"
				}
			]
		}
	],
	"systems": [
		"FlyCamSystem",
		"SpinnerSystem"
	]
}