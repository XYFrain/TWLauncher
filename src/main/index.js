import { app, BrowserWindow } from "electron"
import { join } from "node:path"
import { fileURLToPath } from "node:url"

const __dirname = fileURLToPath(new URL(".", import.meta.url))

app.whenReady().then(() => {
  const win = new BrowserWindow()
  if (process.env.ELECTRON_RENDERER_URL) {
    win.loadURL(process.env.ELECTRON_RENDERER_URL)
  } else {
    win.loadFile(join(__dirname, "../renderer/index.html"))
  }
})

app.on("window-all-closed", () => app.quit())
