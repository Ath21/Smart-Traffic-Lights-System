import { defineStore } from "pinia"
import { searchLogs, exportLogs } from "../services/logApi"
import { useAuth } from "./userStore"

export const useLogStore = defineStore("logStore", {
  state: () => ({
    logs: [],
    isLoading: false,
    error: null,
    filters: {
      Layer: "",
      Service: "",
      Type: "",
      From: "",
      To: ""
    }
  }),

  actions: {
    async fetchLogs(filtersOverride = null) {
      const auth = useAuth()
      if (!auth.user || auth.user.role.toLowerCase() !== "admin") return

      this.isLoading = true
      try {
        const filters = filtersOverride || this.filters
        const data = await searchLogs(filters)

        // Ensure data is an array
        const logsArray = Array.isArray(data) ? data : data.logs || []

        // Map API fields to lowercase keys for table
        this.logs = logsArray.map(log => ({
          id: log.id,
          timestamp: log.Timestamp || log.timestamp || "",
          layer: log.Layer || log.layer || "",
          service: log.Service || log.service || "",
          type: log.Type || log.type || "",
          message: log.Message || log.message || ""
        }))

        this.error = null
      } catch (err) {
        this.error = err.message || "Failed to fetch logs"
      } finally {
        this.isLoading = false
      }
    },

    async export(format = "csv") {
      try {
        const blob = await exportLogs(this.filters, format)

        const now = new Date()
        const pad = (n) => n.toString().padStart(2, "0")
        const dateStr = `${now.getFullYear()}-${pad(now.getMonth() + 1)}-${pad(now.getDate())}_${pad(now.getHours())}-${pad(now.getMinutes())}-${pad(now.getSeconds())}`

        const url = window.URL.createObjectURL(blob)
        const link = document.createElement("a")
        link.href = url
        link.download = `logs_export_${dateStr}.${format}`
        document.body.appendChild(link)
        link.click()
        link.remove()
        window.URL.revokeObjectURL(url)
      } catch (err) {
        this.error = err.message || "Failed to export logs"
      }
    },

    setFilter(key, value) {
      this.filters[key] = value
    },

    clearFilters() {
      this.filters = { Layer: "", Service: "", Type: "", From: "", To: "" }
    }
  }
})
