import { useEffect, useState } from "react";
import api from "../api/api";
import { Link } from "react-router-dom";

export default function StudentSchedulePage() {
    const [schedule, setSchedule] = useState([]);
    const [message, setMessage] = useState("");
    const [errorMessage, setErrorMessage] = useState("");
    const [loading, setLoading] = useState(false);

    useEffect(() => {
        loadSchedule();
    }, []);

    const loadSchedule = async () => {
        try {
            const response = await api.get("/student/schedule");
            setSchedule(response.data);
        } catch (error) {
            console.error(error);
            setErrorMessage("Failed to load SmartSched schedule.");
        }
    };

    const generateSchedule = async () => {
        try {
            setLoading(true);
            await api.post("/student/generate-schedule");
            setMessage("Schedule generated successfully.");
            setErrorMessage("");
            await loadSchedule();
        } catch (error) {
            console.error(error);
            setErrorMessage(error.response?.data?.message || "Failed to generate schedule.");
            setMessage("");
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="page-container">
            <div className="admin-header">
                <div>
                    <h2>SmartSched Schedule</h2>
                    <p>See when SmartSched suggests you should work on tasks.</p>
                </div>
                <div style={{ display: "flex", gap: "10px" }}>
                    <button onClick={generateSchedule} disabled={loading}>
                        {loading ? "Generating..." : "Generate Schedule"}
                    </button>
                    <Link to="/student">Back</Link>
                </div>
            </div>

            {message && <p className="success">{message}</p>}
            {errorMessage && <p className="error">{errorMessage}</p>}

            <div className="admin-section">
                {schedule.length === 0 ? (
                    <p>No schedule generated yet. Click “Generate Schedule”.</p>
                ) : (
                    <div className="compact-list">
                        {schedule.map((item) => (
                            <div key={item.id} className="compact-list-item">
                                <div>
                                    <strong>{item.taskTitle}</strong>
                                    <div className="subtext">{item.course}</div>
                                    <div className="subtext">
                                        {new Date(item.scheduledDate).toLocaleDateString()}
                                    </div>
                                    <div className="subtext">
                                        {item.startTime} - {item.endTime}
                                    </div>
                                    <div className="subtext">
                                        {item.allocatedHours} hour(s)
                                    </div>
                                </div>
                            </div>
                        ))}
                    </div>
                )}
            </div>
        </div>
    );
}