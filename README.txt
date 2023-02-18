Go through the instalation process for unity, and the unity packages as described 
here: https://github.com/Unity-Technologies/ml-agents/blob/develop/docs/Installation.md

python installing 
	py -3.9 -m venv venv
	venv\Scripts\activate
	py -m pip install --upgrade pip
	pip3 install torch~=1.7.1 -f https://download.pytorch.org/whl/torch_stable.html
	pip3 install mlagents==0.30.0 
	pip3 install  protobuf==3.20

Once all that is done you can open the project from unity, navigate to the example-rl scene and open it.

Run ml-agents --help to see other parameters you can add to the learning process, like config files
and output logs.

You will also need this unity asset store library: https://assetstore.unity.com/packages/3d/vehicles/air/simple-drone-190684
for the drone models

