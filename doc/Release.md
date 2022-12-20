# Release Process

1. If there is no release branch yet for the current version:  
   `nbgv prepare-release`
2. Switch to the release branch:  
   `git switch release/v1.2`
3. Push the release branch
4. Create a tag and push it afterwards:
    - `nbgv tag`
    - `git push origin v1.2`
5. Build the plugin:
    - `release.bat`
6. Create a release in GitHub from the tag and attach the file `net.planetrenner.simhub.streamDeckPlugin`
7. Push the main branch.
