Phlayer is a tool for the Unity engine that makes working with layers attached to game objects fast, safe, and automatic (in more ways than one).

Layers in Unity are very powerful; they can be used to differentiate objects about as quickly as possible for a computer. And they are widely used in physics engine queries such as raycasts.

There's a very big problem with layers in Unity without Phlayer, however: they are stored internally in an asset which can only be accessed indirectly. Not only is this slower than how it can be, but there can be outdated references which can cause painful bugs to arise, and more.

Phlayer solves this by automatically generating a script containing all of the layers each time they are edited in the Layer Inspector. Here are all the benefits that Phlayer brings: Robustness - compiler time errors will be generated when layers have been changed but code has not. There won't be any unexpected bugs to track down 
