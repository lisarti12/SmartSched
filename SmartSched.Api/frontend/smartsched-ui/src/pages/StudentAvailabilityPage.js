import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import api from "../api/api";

export default function StudentAvailabilityPage() {
    const [availability, setAvailability] = useState([]);
    const [holidays, setHolidays] = useState([]);
    const [message, setMessage] = useState("");
    const [errorMessage, setErrorMessage] = useState("");
    const [savingAvailability, setSavingAvailability] = useState(false);
    const [savingHoliday, setSavingHoliday] = useState(false);
    const [syncing, setSyncing] = useState(false);

    const [availabilityForm, setAvailabilityForm] = useState({
        availableDate: "",
        startTime: "09:00",
        endTime: "11:00"
    });

    const [holidayForm, setHolidayForm] = useState({
        title: "",
        description: "",
        startDate: "",
        endDate: ""
    });

    useEffect(() => {
        loadData();
    }, []);

    async function loadData() {
        try {
            const [availabilityRes, holidaysRes] = await Promise.all([
                api.get("/student/availability"),
                api.get("/student/holidays")
            ]);

            setAvailability(Array.isArray(availabilityRes.data) ? availabilityRes.data : []);
            setHolidays(Array.isArray(holidaysRes.data) ? holidaysRes.data : []);
        } catch (error) {
            console.error(error);
            setErrorMessage("Failed to load availability and holidays.");
        }
    }

    function clearMessages() {
        setMessage("");
        setErrorMessage("");
    }

    function getTodayLocalDate() {
        const now = new Date();
        const offset = now.getTimezoneOffset();
        const local = new Date(now.getTime() - offset * 60000);
        return local.toISOString().split("T")[0];
    }

    async function saveAvailability(e) {
        e.preventDefault();
        clearMessages();

        const today = getTodayLocalDate();

        if (!availabilityForm.availableDate) {
            setErrorMessage("Please choose a date.");
            return;
        }

        if (availabilityForm.availableDate < today) {
            setErrorMessage("You cannot add availability for a past day.");
            return;
        }

        if (!availabilityForm.startTime || !availabilityForm.endTime) {
            setErrorMessage("Please choose both start time and end time.");
            return;
        }

        if (availabilityForm.endTime <= availabilityForm.startTime) {
            setErrorMessage("End time must be after start time.");
            return;
        }

        try {
            setSavingAvailability(true);

            const response = await api.post("/student/availability", {
                availableDate: availabilityForm.availableDate,
                startTime: `${availabilityForm.startTime}:00`,
                endTime: `${availabilityForm.endTime}:00`
            });

            setMessage(response.data?.message || "Availability saved successfully.");
            setAvailabilityForm({
                availableDate: "",
                startTime: "09:00",
                endTime: "11:00"
            });

            await loadData();
        } catch (error) {
            console.error(error);

            const apiMessage = error.response?.data?.message;
            const apiErrors = error.response?.data?.errors;

            if (apiErrors) {
                const flatErrors = Object.values(apiErrors).flat().join(" ");
                setErrorMessage(flatErrors || apiMessage || "Failed to save availability.");
            } else {
                setErrorMessage(apiMessage || "Failed to save availability.");
            }
        } finally {
            setSavingAvailability(false);
        }
    }

    async function deleteAvailability(id) {
        clearMessages();

        if (!window.confirm("Remove this availability slot?")) {
            return;
        }

        try {
            await api.delete(`/student/availability/${id}`);
            setMessage("Availability removed.");
            await loadData();
        } catch (error) {
            console.error(error);
            setErrorMessage(error.response?.data?.message || "Failed to remove availability.");
        }
    }

    async function addHoliday(e) {
        e.preventDefault();
        clearMessages();

        if (!holidayForm.title.trim()) {
            setErrorMessage("Holiday title is required.");
            return;
        }

        if (!holidayForm.startDate || !holidayForm.endDate) {
            setErrorMessage("Start date and end date are required.");
            return;
        }

        if (holidayForm.endDate < holidayForm.startDate) {
            setErrorMessage("Holiday end date must be after start date.");
            return;
        }

        try {
            setSavingHoliday(true);

            const response = await api.post("/student/holidays", {
                title: holidayForm.title.trim(),
                description: holidayForm.description.trim(),
                startDate: holidayForm.startDate,
                endDate: holidayForm.endDate
            });

            setMessage(response.data?.message || "Holiday added.");
            setHolidayForm({
                title: "",
                description: "",
                startDate: "",
                endDate: ""
            });

            await loadData();
        } catch (error) {
            console.error(error);
            setErrorMessage(error.response?.data?.message || "Failed to add holiday.");
        } finally {
            setSavingHoliday(false);
        }
    }

    async function deleteHoliday(id) {
        clearMessages();

        if (!window.confirm("Remove this holiday?")) {
            return;
        }

        try {
            await api.delete(`/student/holidays/${id}`);
            setMessage("Holiday removed.");
            await loadData();
        } catch (error) {
            console.error(error);
            setErrorMessage(error.response?.data?.message || "Failed to remove holiday.");
        }
    }

    async function syncHolidays() {
        clearMessages();

        try {
            setSyncing(true);
            const year = new Date().getFullYear();
            const response = await api.post(`/student/holidays/sync/${year}`);
            setMessage(response.data?.message || "Holiday sync completed.");
            await loadData();
        } catch (error) {
            console.error(error);
            setErrorMessage(error.response?.data?.message || "Failed to sync holidays.");
        } finally {
            setSyncing(false);
        }
    }

    function formatDate(dateValue) {
        return new Date(dateValue).toLocaleDateString();
    }

    function formatTime(timeValue) {
        if (!timeValue) return "";
        return String(timeValue).slice(0, 5);
    }

    return (
        <div className="page-container">
            <div className="admin-header" style={{ alignItems: "flex-start" }}>
                <div style={{ display: "flex", flexDirection: "column", gap: "8px" }}>
                    <Link to="/student" style={{ alignSelf: "flex-start" }}>← Back</Link>
                    <div>
                        <h2>Availability & Holidays</h2>
                        <p>Add the exact dates and time ranges when you are available to work.</p>
                    </div>
                </div>

                <button onClick={syncHolidays} disabled={syncing}>
                    {syncing ? "Syncing..." : "Sync Holidays"}
                </button>
            </div>

            {message && <p className="success">{message}</p>}
            {errorMessage && <p className="error">{errorMessage}</p>}

            <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "20px" }}>
                <div className="admin-section">
                    <h3>Create Availability</h3>

                    <form onSubmit={saveAvailability}>
                        <div className="form-group">
                            <label>Date</label>
                            <input
                                type="date"
                                min={getTodayLocalDate()}
                                value={availabilityForm.availableDate}
                                onChange={(e) =>
                                    setAvailabilityForm({ ...availabilityForm, availableDate: e.target.value })
                                }
                                required
                            />
                        </div>

                        <div className="form-group">
                            <label>Start time</label>
                            <input
                                type="time"
                                value={availabilityForm.startTime}
                                onChange={(e) =>
                                    setAvailabilityForm({ ...availabilityForm, startTime: e.target.value })
                                }
                                required
                            />
                        </div>

                        <div className="form-group">
                            <label>End time</label>
                            <input
                                type="time"
                                value={availabilityForm.endTime}
                                onChange={(e) =>
                                    setAvailabilityForm({ ...availabilityForm, endTime: e.target.value })
                                }
                                required
                            />
                        </div>

                        <button type="submit" disabled={savingAvailability}>
                            {savingAvailability ? "Saving..." : "Save Availability"}
                        </button>
                    </form>

                    <div style={{ marginTop: "20px" }}>
                        <h4>Saved Availability</h4>

                        {availability.length === 0 ? (
                            <p>No availability slots yet.</p>
                        ) : (
                            <div className="compact-list">
                                {availability.map((slot) => (
                                    <div key={slot.id} className="compact-list-item">
                                        <div>
                                            <strong>{formatDate(slot.availableDate)}</strong>
                                            <div className="subtext">{slot.dayOfWeek}</div>
                                            <div className="subtext">
                                                {formatTime(slot.startTime)} - {formatTime(slot.endTime)}
                                            </div>
                                        </div>

                                        <button
                                            className="danger-btn"
                                            onClick={() => deleteAvailability(slot.id)}
                                        >
                                            Remove
                                        </button>
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>
                </div>

                <div className="admin-section">
                    <h3>Add Holiday / Unavailability</h3>

                    <form onSubmit={addHoliday}>
                        <div className="form-group">
                            <label>Title</label>
                            <input
                                value={holidayForm.title}
                                onChange={(e) =>
                                    setHolidayForm({ ...holidayForm, title: e.target.value })
                                }
                                required
                            />
                        </div>

                        <div className="form-group">
                            <label>Description</label>
                            <textarea
                                rows="3"
                                value={holidayForm.description}
                                onChange={(e) =>
                                    setHolidayForm({ ...holidayForm, description: e.target.value })
                                }
                                style={{
                                    width: "100%",
                                    padding: "12px",
                                    borderRadius: "8px",
                                    border: "1px solid #d1d5db"
                                }}
                            />
                        </div>

                        <div className="form-group">
                            <label>Start date</label>
                            <input
                                type="date"
                                value={holidayForm.startDate}
                                onChange={(e) =>
                                    setHolidayForm({ ...holidayForm, startDate: e.target.value })
                                }
                                required
                            />
                        </div>

                        <div className="form-group">
                            <label>End date</label>
                            <input
                                type="date"
                                value={holidayForm.endDate}
                                onChange={(e) =>
                                    setHolidayForm({ ...holidayForm, endDate: e.target.value })
                                }
                                required
                            />
                        </div>

                        <button type="submit" disabled={savingHoliday}>
                            {savingHoliday ? "Saving..." : "Add Holiday"}
                        </button>
                    </form>

                    <div style={{ marginTop: "20px" }}>
                        <h4>Saved Holidays</h4>

                        {holidays.length === 0 ? (
                            <p>No holidays yet.</p>
                        ) : (
                            <div className="compact-list">
                                {holidays.map((holiday) => (
                                    <div key={holiday.id} className="compact-list-item">
                                        <div>
                                            <strong>{holiday.title}</strong>
                                            <div className="subtext">
                                                {formatDate(holiday.startDate)} - {formatDate(holiday.endDate)}
                                            </div>
                                            {holiday.description && (
                                                <div className="subtext">{holiday.description}</div>
                                            )}
                                        </div>

                                        <button
                                            className="danger-btn"
                                            onClick={() => deleteHoliday(holiday.id)}
                                        >
                                            Remove
                                        </button>
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
}