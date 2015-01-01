Biterator
=========

A simple class that packs bits into a byte array. Ideally for compressing data to be sent over a network.

## Currently allows support for the following data types:

* **int** - define amount of bits needed, Range [2, 32]
* **uint** - define amount of bits needed, Range [1, 32]
* **bool** - compresses to 1 bit 
* **float** - compress signed bit along with the precision of the significand/matissa component, Mantissa Range [1, 23], Total Size Range [9, 32]

## How to use

A Biterator works off of the user's assumptions about their data. If you know the range of a given variable, you can predict the maximum number of bits you will need to properly represent the value.

For example, if you have a variable that you know will always be between 0 and 10, you know that you only need to use an unsigned integer. You also know that you only need 4 bits to represent all possible values (10 in binary = 1010).

When you pack data into a Biterator and send it over a network, you need to make sure you unpack it in the same fashion. If you don't match your Push and Pop commands from a Biterator your data will appear incorrect upon unpacking.

## Examples

Add and compress elements into a Biterator

```csharp
uint score = 10;
bool gameOver = false;

//Create a Biterator that can hold up to 2 bytes of data
Biterator biterator = new Biterator(2);

biterator.PushUInt(score, 4); //assume this variable will never exceed 4 bits
biterator.PushBool(gameOver);
```

Unpack and decompress elements from a Biterator

```csharp
byte[] data;//this data will have been sent to you over a network

//Initialize a biterator with a byte array
Biterator biterator = new Biterator(data);

uint score = biterator.PopUInt(4);
bool gameOver = biterator.PopBool();
```

## Unity Utils

Print biterator bytes with color coded elements.
Compatible color strings can be found here: http://docs.unity3d.com/Manual/StyledText.html

```csharp
uint score = 10;
bool gameOver = false;

//create a biterator that can hold up to 2 bytes of data
Biterator biterator = new Biterator(2);

biterator.PushUInt(score, 4);
biterator.PushBool(gameOver);

string[] colors = new string[]{"aqua", "yellow", "lime", "magenta"};
BiteratorUnityUtils.DebugPrintBits(biterator, colors);
```