all: LevelDB.dll

LevelDB.dll: ByteBuffer.cs Coding.cs Random.cs SkipList.cs Slice.cs
	gmcs -target:library -debug+ -out:LevelDB.dll ByteBuffer.cs Coding.cs Random.cs SkipList.cs Slice.cs
