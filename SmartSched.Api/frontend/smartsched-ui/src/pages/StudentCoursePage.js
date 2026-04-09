import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import api from "../api/api";

export default function StudentCoursePage() {
    const { id } = useParams();

    const [courseData, setCourseData] = useState(null);
    const [message, setMessage] = useState("");
    const [errorMessage, setErrorMessage] = useState("");
    const [taskForms, setTaskForms] = useState({});

    useEffect(() => {
        loadCourse();
    }, [id]);

    const loadCourse = async () => {
        try {
            const response = await api.get(`/student/courses/${id}`);
            setCourseData(response.data);
        } catch (error) {
            console.error(error);
            setErrorMessage("Failed to load course.");
        }
    };

    const handleFormChange = (contentId, field, value) => {
        setTaskForms((prev) => ({
            ...prev,
            [contentId]: {
                ...prev[contentId],
                [field]: value
            }
        }));
    };

    const createAsTask = async (contentId) => {
        const form = taskForms[contentId] || {};
        const estimatedHours = parseInt(form.estimatedHours);
        const priority = parseInt(form.priority);
        const difficulty = parseInt(form.difficulty);

        if (!estimatedHours || estimatedHours < 1 || estimatedHours > 12) {
            setErrorMessage("Estimated hours must be between 1 and 12.");
            return;
        }

        if (!priority || priority < 1 || priority > 5) {
            setErrorMessage("Priority must be between 1 and 5.");
            return;
        }

        if (!difficulty || difficulty < 1 || difficulty > 5) {
            setErrorMessage("Difficulty must be between 1 and 5.");
            return;
        }

        try {
            const response = await api.post(`/student/course-content/${contentId}/create-task`, {
                estimatedHours,
                priority,
                difficulty
            });

            setMessage(response.data.warning || response.data.message);
            setErrorMessage("");
            loadCourse();
        } catch (error) {
            console.error(error);
            setErrorMessage(error.response?.data?.message || "Failed to create SmartSched task.");
            setMessage("");
        }
    };

    if (!courseData) return <div className="page-container">Loading...</div>;

    return (
        <div className="page-container">
            <div className="admin-header">
                <div>
                    <h2>{courseData.course.title}</h2>
                    <p>{courseData.course.semester}</p>
                </div>
                <Link to="/student">Back</Link>
            </div>

            {message && <p className="success">{message}</p>}
            {errorMessage && <p className="error">{errorMessage}</p>}

            <div className="admin-section">
                <h3>Active Assignments / Quizzes / Projects</h3>

                {courseData.activeContent.length === 0 ? (
                    <p>No active class content.</p>
                ) : (
                    <div className="compact-list">
                        {courseData.activeContent.map((item) => (
                            <div key={item.id} className="compact-list-item" style={{ alignItems: "flex-start" }}>
                                <div style={{ flex: 1 }}>
                                    <strong>[{item.type}] {item.title}</strong>
                                    <div className="subtext">{item.description}</div>
                                    <div className="subtext">Due: {new Date(item.dueDate).toLocaleString()}</div>

                                    {item.filePath && (
                                        <div className="subtext">
                                            <a href={`https://localhost:7189${item.filePath}`} target="_blank" rel="noreferrer">
                                                Download file
                                            </a>
                                        </div>
                                    )}

                                    {!item.alreadyImported && (
                                        <div style={{ marginTop: "10px", display: "grid", gridTemplateColumns: "repeat(3, 1fr)", gap: "8px" }}>
                                            <input
                                                type="number"
                                                placeholder="Hours"
                                                min="1"
                                                max="12"
                                                onChange={(e) => handleFormChange(item.id, "estimatedHours", e.target.value)}
                                            />
                                            <input
                                                type="number"
                                                placeholder="Priority"
                                                min="1"
                                                max="5"
                                                onChange={(e) => handleFormChange(item.id, "priority", e.target.value)}
                                            />
                                            <input
                                                type="number"
                                                placeholder="Difficulty"
                                                min="1"
                                                max="5"
                                                onChange={(e) => handleFormChange(item.id, "difficulty", e.target.value)}
                                            />
                                        </div>
                                    )}
                                </div>

                                <div style={{ marginLeft: "12px" }}>
                                    {item.alreadyImported ? (
                                        <span className="subtext">Added to SmartSched</span>
                                    ) : (
                                        <button onClick={() => createAsTask(item.id)}>
                                            Create as SmartSched Task
                                        </button>
                                    )}
                                </div>
                            </div>
                        ))}
                    </div>
                )}
            </div>

            <div className="admin-section">
                <h3>Previous Content</h3>

                {courseData.previousContent.length === 0 ? (
                    <p>No previous items.</p>
                ) : (
                    <div className="compact-list">
                        {courseData.previousContent.map((item) => (
                            <div key={item.id} className="compact-list-item">
                                <div>
                                    <strong>[{item.type}] {item.title}</strong>
                                    <div className="subtext">{item.description}</div>
                                    <div className="subtext">Due: {new Date(item.dueDate).toLocaleString()}</div>

                                    {item.filePath && (
                                        <div className="subtext">
                                            <a href={`https://localhost:7189${item.filePath}`} target="_blank" rel="noreferrer">
                                                Download file
                                            </a>
                                        </div>
                                    )}
                                </div>
                            </div>
                        ))}
                    </div>
                )}
            </div>

            <div className="admin-section">
                <h3>Lectures</h3>

                {courseData.lectures.length === 0 ? (
                    <p>No lectures yet.</p>
                ) : (
                    <div className="compact-list">
                        {courseData.lectures.map((lecture) => (
                            <div key={lecture.id} className="compact-list-item">
                                <div>
                                    <strong>{lecture.title}</strong>
                                    {lecture.filePath && (
                                        <div className="subtext">
                                            <a href={`https://localhost:7189${lecture.filePath}`} target="_blank" rel="noreferrer">
                                                Download file
                                            </a>
                                        </div>
                                    )}
                                </div>
                            </div>
                        ))}
                    </div>
                )}
            </div>
        </div>
    );
}