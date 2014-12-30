Biterator
=========

A simple class that packs bits into a byte array. Ideally for compressing data to be sent over a network.

## Currently allows support for the following data types:

bool - compresses to 1 byte
uint - define amount of bits needed
int - define amount of bits needed
float - define if value should be signed along with the precision of the significand/matissa component

## Examples

Add and compress elements into a Biterator

```csharp
int score = 10;
bool gameOver = false;

//create a biterator that can hold up to 4 bytes of data
Biterator biterator = new Biterator(4);

biterator.PushInt(score, 4);
biterator.PushBool(gameOver);
```

Unpack and decompress elements from a Biterator

```csharp
//create a biterator that can hold up to 4 bytes of data

byte[] data;//this data will have been sent to you over a network

Biterator biterator = new Biterator(data);

int score = biterator.PopInt(4);
bool gameOver = biterator.PopBool();
```

## Unity Utils

Print biterator bytes with color coded elements.
Compatible color strings can be found here: http://docs.unity3d.com/Manual/StyledText.html

```csharp
int score = 10;
bool gameOver = false;

//create a biterator that can hold up to 4 bytes of data
Biterator biterator = new Biterator(4);

biterator.PushInt(score, 4);
biterator.PushBool(gameOver);

string[] colors = new string[]{"aqua", "yellow", "lime", "magenta"};
BiteratorUnityUtils.DebugPrintBits(biterator, colors);
```