// Browser helpers shared by the Blazor Server and WebAssembly hosts.
// Storage can be disabled, full, or blocked by privacy settings; every helper degrades instead of throwing.
function storage() {
    try {
        return window.localStorage ?? null;
    } catch {
        return null;
    }
}

window.learningPreferences = {
    read() {
        const fallback = {
            isDarkMode: matchMedia("(prefers-color-scheme: dark)").matches,
            drawerOpen: true,
            available: false
        };
        try {
            const store = storage();
            if (!store) return fallback;
            const theme = store.getItem("isDarkMode");
            const drawer = store.getItem("drawerOpen");
            return {
                isDarkMode: theme === null ? fallback.isDarkMode : theme.toLowerCase() === "true",
                drawerOpen: drawer === null || drawer.toLowerCase() === "true",
                available: true
            };
        } catch {
            return fallback;
        }
    },
    write(key, value) {
        try {
            const store = storage();
            if (!store) return false;
            store.setItem(key, String(value));
            return true;
        } catch {
            return false;
        }
    }
};

window.learningFiles = {
    async copy(text) {
        try {
            await navigator.clipboard.writeText(text);
            return true;
        } catch {
            return false;
        }
    },
    download(name, text) {
        const url = URL.createObjectURL(new Blob([text], { type: "text/csv;charset=utf-8" }));
        const link = document.createElement("a");
        link.href = url;
        link.download = name;
        link.click();
        setTimeout(() => URL.revokeObjectURL(url), 1000);
    }
};

// Static WebAssembly demo only: the culture is chosen before the .NET runtime starts.
window.learningCulture = {
    get() {
        try {
            return storage()?.getItem("culture") ?? null;
        } catch {
            return null;
        }
    },
    set(culture) {
        try {
            const store = storage();
            if (!store) return false;
            store.setItem("culture", culture);
            return true;
        } catch {
            return false;
        }
    },
    applyDocumentLanguage(language) {
        document.documentElement.lang = language;
    }
};

// Static WebAssembly demo only: a localStorage notebook with the same contract as the SQLite one.
// Each operation is a synchronous compare-and-swap, so two tabs still produce version conflicts.
const notebookKey = "learning.notebook";

function readNotes() {
    const store = storage();
    if (!store) throw new Error("Browser storage is unavailable.");
    const notes = JSON.parse(store.getItem(notebookKey) ?? "[]");
    return Array.isArray(notes) ? notes : [];
}

function writeNotes(notes) {
    storage().setItem(notebookKey, JSON.stringify(notes));
}

window.learningNotebook = {
    list() {
        return readNotes();
    },
    insert(note, limit) {
        const notes = readNotes();
        if (notes.length >= limit) return "limit";
        notes.push(note);
        writeNotes(notes);
        return "ok";
    },
    update(note, expectedVersion) {
        const notes = readNotes();
        const index = notes.findIndex(item => item.id === note.id);
        if (index < 0 || notes[index].version !== expectedVersion) return "conflict";
        notes[index] = note;
        writeNotes(notes);
        return "ok";
    },
    remove(id, expectedVersion) {
        const notes = readNotes();
        const index = notes.findIndex(item => item.id === id);
        if (index < 0 || notes[index].version !== expectedVersion) return "conflict";
        notes.splice(index, 1);
        writeNotes(notes);
        return "ok";
    },
    reset() {
        writeNotes([]);
    }
};
