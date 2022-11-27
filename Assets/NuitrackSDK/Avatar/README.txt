Namespace: NuitrackSDK.Avatar

Avatar class structure

	* BaseAvatar (abstract class)
		Base type providing simple functions for obtaining data about the skeleton.
		If you want to write your own Avatar controller, it is recommended to inherit all avatars from it.

		Inherits TrackedUser.

	* NuitrackAvatar
		Avatar for 3D models.
		Supports alignment of bone length to the real skeleton and customization for use in VR.
		It has a simple setup and mapping of the skeleton.
		This will suit you for most cases.
	
		Inherits BaseAvatar.
 
	* Skeleton Avatar
		A component for displaying a 3D skeleton using primitives for joints and bones.
		Inherits BaseAvatar.

	* UIAvatar
		A component for displaying a 2D (UI) skeleton using primitives for joints and bones.
		Inherits BaseAvatar.


Namespace: NuitrackSDKEditor.Avatar
	If you want to write your own Avatar controller, then you will most likely need to make a GUI shell for Editor for it.
	We have prepared a set of components for mapping \ automapping, working with the skeleton, 
	and a lot of GUI components to facilitate development.


Editor сlass structure

	* NuitrackSDKEditor (abstract class)
		Base type from which to inherit inspectors for NuitrackSDK (hides all attributes marked as NuitrackSDKInspector)

	* BaseAvatarEditor 
		Editor for BaseAvatar. 
		The base type from which the inspectors for Avatar are inherited. For your own Avatars that inherit from BaseAvatar,
		the editor scripts should also be inherited from BaseAvatarEditor.

		Inherits NuitrackSDKEditor.

	* NuitrackAvatarEditor
		Editor for NuitrackAvatar.
		If you want to write your own avatar controller, you can study this script.

		Inherits BaseAvatarEditor.


Utils

	* SkeletonMapperGUI / SkeletonMapperListGUI 
		Mapping / List drawing for the skeleton in the likeness Avatar Configuration (humanoid models)
		Can be used in any inspector scripts and work with a given type of target objects (in Avatar it is Transform)
		Supports mask mapping, optional joints, highlighting of a body part for which all mandatory joints are not specified.

		Inherited from SkeletonMapper, which combines a couple of common properties and methods.

	* SkeletonBonesView
		Drawing a skeleton on scene.
		Supports two modes: bone mapping for the original humanoid model (Model bones) 
		and assigned bones for Avatar (Assigned bones).
		
		For the "Model bones" mode, it implements the assignment of new bones from an object on the scene.
		For the "Assigned bones" mode, it implements the selection and removal of assigned bones.

	* SkeletonUtils
		Utilities for automatic mapping, bone detection (Transform) of a humanoid model and setting the model in T-Pose.
		See more in docstrings for Skeleton Utils

	For advanced scripting for Editor GUI, see also NuitrackSDKGUI.