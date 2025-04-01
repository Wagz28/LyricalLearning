---
created: 2025-04-01T14:06:40.685Z
updated: 2025-04-01T14:16:24.805Z
assigned: ""
progress: 1
tags: []
started: 2025-04-01T00:00:00.000Z
completed: 2025-04-01T14:16:24.805Z
---

# Investigating Flat file vs DB

Okay so flat file won't scale well and seems to be much more arduous format wise: thinking about all the different word, sentence and paragraph translating.
Instead using a DB will be easy formatting, a little more effort to set up but a lot cleaner and very scalabe, a sound plan is to use SQLite for now and then if we ever need or want to change we can switch and this will require minimal changes overall/comparatively. The concept stays the same in scaling this way: possibillities to dynamically edit, change and add songs or translations, if we ever get to the point we could add user chosen song uploads.

## Relations

- [Conditional sprint-1](sprint-1.md)
