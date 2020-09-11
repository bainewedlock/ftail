## Usage
Ftail.exe (filename)

The App is designed to be called from another app (like FreeCommander) passing the path of a log file.


## Configuration
Create /users/<username>/ftail.xml with following content:
  
```  
<FTailConfig>
  <InteractiveHighlightForeground>White</InteractiveHighlightForeground>
  <InteractiveHighlightBackground>Teal</InteractiveHighlightBackground>
  <Font>
    <Family>Fira Code Medium</Family>
    <Size>11</Size>
  </Font>
  <Filter>
    <Pattern>a tag to highlight always|another one</Pattern>
    <Foreground>Black</Foreground>
    <Background>Yellow</Background>
  </Filter>
  <Filter>
    <Pattern>\[error\]</Pattern>
    <Foreground>Black</Foreground>
    <Background>Orange</Background>
  </Filter>
  <Filter>
    <Pattern>\[warn\]</Pattern>
    <Foreground>Black</Foreground>
    <Background>Fuchsia</Background>
  </Filter>
  <Filter>
    <Pattern>\[info\]</Pattern>
    <Foreground>Black</Foreground>
    <Background>White</Background>
  </Filter>
  <Filter>
    <Pattern>\[debug\]</Pattern>
    <Foreground>Gray</Foreground>
    <Background>White</Background>
  </Filter>
</FTailConfig>

```
