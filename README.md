# CharacterSwappingScript
Adds the ability to switch between playable characters during gameplay in Batman: Arkham City

## ⚠️ Notice: In active development
_This mod is currently under construction! Some functionality might be unstable or missing entirely._

## How to install
1. Get the [BmSDK](https://github.com/etkramer/BmSDK?tab=readme-ov-file#-getting-started-for-users)
2. Clone the repo into `%GameDir%\BmGame\Scripts` by either:
   - using `git clone https://github.com/Samuil1337/CharacterSwappingScript.git` in the target directory
   - or downloading and extracting [the latest ZIP](https://github.com/Samuil1337/CharacterSwappingScript/archive/refs/heads/main.zip) in the target directory
3. Enjoy!

## How to use
Start the modded game instance through the usual means and just press the **function keys** to switch between the available characters:
- Batman (**F1**)
- Catwoman (**F2**)
- Robin (**F3**)
- Nightwing (**F4**)
- Bruce Wayne (**F5**)

## How to configure
All the configuration options are constants at the top of **CharacterSwappingScript.cs**, with inline documentation explaining what each variable does. Though, it is recommended to stick to the defaults for the smoothest experience.

## Develoment of this mod
<details>
<summary>Todo to for the full release</summary>

### Feature:
- [x] Implement basic swapping mechanic
- [x] Add proper shared health pool
- [x] Add smoke bomb transition on switch
- [ ] Add proper damage level support
- [ ] Prevent infinite gadgets when switching characters
- [ ] Transfer over movement states #3
- [ ] *Optional: Give Nightwing the REC gun in slot 7*

### Performance:
- [ ] Load in all characters once at init and reuse assets
- [ ] *Optional: Reduce visibility of texture streaming on switch*

### Bug Fixes:
The known bugs reside on the [issue tracker](https://github.com/Samuil1337/CharacterSwappingScript/issues).

</details>
