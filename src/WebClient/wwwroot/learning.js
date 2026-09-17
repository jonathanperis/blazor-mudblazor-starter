window.learningPreferences = {
    read() {
        try {
            const theme = localStorage.getItem("isDarkMode");
            const drawer = localStorage.getItem("drawerOpen");
            return {
                isDarkMode: theme === null ? matchMedia("(prefers-color-scheme: dark)").matches : theme.toLowerCase() === "true",
                drawerOpen: drawer === null || drawer.toLowerCase() === "true",
                available: true
            };
        } catch (error) {
            if (error.name !== "SecurityError") throw error;
            return { isDarkMode: false, drawerOpen: true, available: false };
        }
    },
    write(key, value) {
        try {
            localStorage.setItem(key, String(value));
            return true;
        } catch (error) {
            if (!["SecurityError", "QuotaExceededError"].includes(error.name)) throw error;
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
