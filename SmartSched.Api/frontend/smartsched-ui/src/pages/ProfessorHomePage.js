import { useEffect, useState } from "react";
import api from "../api/api";
import { Link } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

export default function ProfessorHomePage() {
    const { user, logout } = useAuth();

    const [classes, setClasses] = useState([]);
    const [selectedClass, setSelectedClass] = useState(null);
    const [classDetails, setClassDetails] = useState(null);
    const [availableStudents, setAvailableStudents] = useState([]);
    const [selectedStudents, setSelectedStudents] = useState([]);
    const [message, setMessage] = useState("");
    const [errorMessage, setErrorMessage] = useState("");
    const [unreadChatCount, setUnreadChatCount] = useState(0);

    const [classForm, setClassForm] = useState({
        title: "",
        description: "",
        semester: "Fall 26"
    });

    const [classErrors, setClassErrors] = useState({});

    const [contentForm, setContentForm] = useState({
        type: "Homework",
        title: "",
        description: "",
        dueDate: "",
        file: null
    });

    const [contentErrors, setContentErrors] = useState({});

    const [lectureForm, setLectureForm] = useState({
        title: "",
        file: null
    });

    const [lectureErrors, setLectureErrors] = useState({});

    const loadProfessorData = async () => {
        try {
            const classesRes = await api.get("/professor/classes");
            setClasses(classesRes.data);
        } catch (error) {
            console.error(error);
            setErrorMessage("Failed to load professor data.");
        }
    };

    const loadUnreadChatCount = async () => {
        try {
            const response = await api.get("/chat/unread-total");
            setUnreadChatCount(response.data.unreadTotal || 0);
        } catch (error) {
            console.error("Failed to load unread chat count:", error);
        }
    };

    const loadClassDetails = async (classId) => {
        try {
            const [detailsRes, availableRes] = await Promise.all([
                api.get(`/professor/classes/${classId}`),
                api.get(`/professor/classes/${classId}/available-students`)
            ]);

            setSelectedClass(classId);
            setClassDetails(detailsRes.data);
            setAvailableStudents(availableRes.data);
            setSelectedStudents([]);
        } catch (error) {
            console.error(error);
            setErrorMessage("Failed to load class details.");
        }
    };

    useEffect(() => {
        loadProfessorData();
        loadUnreadChatCount();

        const interval = setInterval(() => {
            loadUnreadChatCount();
        }, 3000);

        return () => clearInterval(interval);
    }, []);

    const validateClassForm = () => {
        const errors = {};

        if (!classForm.title.trim()) {
            errors.title = "Class title is required.";
        } else if (!/^[A-Z]{3}\d{4}[A-Z]\s.+$/.test(classForm.title.trim())) {
            errors.title = "Use format of major acronym, a 4-digit number and letter then name of the course.";
        }

        if (!classForm.description.trim()) {
            errors.description = "Course description is required.";
        } else if (classForm.description.trim().length < 15) {
            errors.description = "Description must be at least 15 characters.";
        }

        if (!classForm.semester) {
            errors.semester = "Semester is required.";
        }

        return errors;
    };

    const validateContentForm = () => {
        const errors = {};

        if (!contentForm.title.trim()) {
            errors.title = "Title is required.";
        }

        if (!contentForm.description.trim()) {
            errors.description = "Description is required.";
        } else if (contentForm.description.trim().length < 10) {
            errors.description = "Description must be at least 10 characters.";
        }

        if (!contentForm.dueDate) {
            errors.dueDate = "Deadline is required.";
        } else if (new Date(contentForm.dueDate) <= new Date()) {
            errors.dueDate = "Deadline must be in the future.";
        }

        return errors;
    };

    const validateLectureForm = () => {
        const errors = {};

        if (!lectureForm.title.trim()) {
            errors.title = "Lecture title is required.";
        }

        return errors;
    };

    const handleDeleteContent = async (contentId) => {
        if (!selectedClass) return;

        const confirmed = window.confirm("Are you sure you want to delete this content item?");
        if (!confirmed) return;

        try {
            await api.delete(`/professor/classes/${selectedClass}/content/${contentId}`);
            setMessage("Content item deleted successfully.");
            setErrorMessage("");
            loadClassDetails(selectedClass);
        } catch (error) {
            console.error(error);
            setErrorMessage("Failed to delete content item.");
            setMessage("");
        }
    };

    const handleDeleteLecture = async (lectureId) => {
        if (!selectedClass) return;

        const confirmed = window.confirm("Are you sure you want to delete this lecture?");
        if (!confirmed) return;

        try {
            await api.delete(`/professor/classes/${selectedClass}/lectures/${lectureId}`);
            setMessage("Lecture deleted successfully.");
            setErrorMessage("");
            loadClassDetails(selectedClass);
        } catch (error) {
            console.error(error);
            setErrorMessage("Failed to delete lecture.");
            setMessage("");
        }
    };

    const handleCreateClass = async (e) => {
        e.preventDefault();

        const errors = validateClassForm();
        setClassErrors(errors);

        if (Object.keys(errors).length > 0) return;

        try {
            await api.post("/professor/classes", classForm);
            setMessage("Class created successfully.");
            setErrorMessage("");
            setClassForm({ title: "", description: "", semester: "Fall 26" });
            setClassErrors({});
            loadProfessorData();
        } catch (error) {
            console.error(error);
            setErrorMessage("Failed to create class.");
            setMessage("");
        }
    };

    const handleAddStudents = async () => {
        if (!selectedClass || selectedStudents.length === 0) return;

        try {
            await api.post(`/professor/classes/${selectedClass}/students`, {
                studentIds: selectedStudents
            });

            setMessage("Students added successfully.");
            setErrorMessage("");
            setSelectedStudents([]);
            loadClassDetails(selectedClass);
            loadProfessorData();
        } catch (error) {
            console.error(error);
            setErrorMessage(error.response?.data?.message || "Failed to add students.");
            setMessage("");
        }
    };

    const handleRemoveStudent = async (studentId) => {
        if (!selectedClass) return;

        try {
            await api.delete(`/professor/classes/${selectedClass}/students/${studentId}`);
            setMessage("Student removed from class.");
            setErrorMessage("");
            loadClassDetails(selectedClass);
            loadProfessorData();
        } catch (error) {
            console.error(error);
            setErrorMessage("Failed to remove student.");
            setMessage("");
        }
    };

    const handleCreateContent = async (e) => {
        e.preventDefault();

        const errors = validateContentForm();
        setContentErrors(errors);

        if (Object.keys(errors).length > 0 || !selectedClass) return;

        try {
            const formData = new FormData();
            formData.append("type", contentForm.type);
            formData.append("title", contentForm.title);
            formData.append("description", contentForm.description);
            formData.append("dueDate", contentForm.dueDate);

            if (contentForm.file) {
                formData.append("file", contentForm.file);
            }

            await api.post(`/professor/classes/${selectedClass}/content`, formData, {
                headers: { "Content-Type": "multipart/form-data" }
            });

            setMessage("Class content created successfully.");
            setErrorMessage("");
            setContentForm({
                type: "Homework",
                title: "",
                description: "",
                dueDate: "",
                file: null
            });
            setContentErrors({});
            loadClassDetails(selectedClass);
        } catch (error) {
            console.error(error);
            setErrorMessage(error.response?.data?.message || "Failed to create content.");
            setMessage("");
        }
    };

    const handleCreateLecture = async (e) => {
        e.preventDefault();

        const errors = validateLectureForm();
        setLectureErrors(errors);

        if (Object.keys(errors).length > 0 || !selectedClass) return;

        try {
            const formData = new FormData();
            formData.append("title", lectureForm.title);

            if (lectureForm.file) {
                formData.append("file", lectureForm.file);
            }

            await api.post(`/professor/classes/${selectedClass}/lectures`, formData, {
                headers: { "Content-Type": "multipart/form-data" }
            });

            setMessage("Lecture created successfully.");
            setErrorMessage("");
            setLectureForm({ title: "", file: null });
            setLectureErrors({});
            loadClassDetails(selectedClass);
        } catch (error) {
            console.error(error);
            setErrorMessage("Failed to create lecture.");
            setMessage("");
        }
    };

    return (
        <div className="page-container">
            <div className="admin-header">
                <div>
                    <h2>Professor Dashboard</h2>
                    <p>Welcome, {user?.fullName}</p>
                </div>

                <div style={{ display: "flex", gap: "10px", alignItems: "center" }}>
                    <Link to="/chat">
                        <button>
                            Open Chat{unreadChatCount > 0 ? ` (${unreadChatCount})` : ""}
                        </button>
                    </Link>

                    <button onClick={logout}>Logout</button>
                </div>
            </div>

            {message && <p className="success">{message}</p>}
            {errorMessage && <p className="error">{errorMessage}</p>}

            <div style={{ display: "grid", gridTemplateColumns: "340px 1fr", gap: "20px" }}>
                <div className="admin-section">
                    <h3>Create Class</h3>

                    <form onSubmit={handleCreateClass}>
                        <div className="form-group">
                            <input
                                placeholder="COS3111A Computer Architecture"
                                value={classForm.title}
                                onChange={(e) => setClassForm({ ...classForm, title: e.target.value })}
                            />
                            {classErrors.title && <p className="field-error">{classErrors.title}</p>}
                        </div>

                        <div className="form-group">
                            <select
                                value={classForm.semester}
                                onChange={(e) => setClassForm({ ...classForm, semester: e.target.value })}
                            >
                                <option value="Fall 26">Fall 26</option>
                                <option value="Spring 27">Spring 27</option>
                            </select>
                            {classErrors.semester && <p className="field-error">{classErrors.semester}</p>}
                        </div>

                        <div className="form-group">
                            <textarea
                                placeholder="Course description"
                                value={classForm.description}
                                onChange={(e) => setClassForm({ ...classForm, description: e.target.value })}
                                rows="4"
                                style={{ width: "100%", padding: "12px", borderRadius: "8px", border: "1px solid #d1d5db" }}
                            />
                            {classErrors.description && <p className="field-error">{classErrors.description}</p>}
                        </div>

                        <button type="submit">Create Class</button>
                    </form>

                    <h3 style={{ marginTop: "24px" }}>My Classes</h3>

                    {classes.length === 0 ? (
                        <p>No classes yet.</p>
                    ) : (
                        <div className="compact-list">
                            {classes.map((course) => (
                                <div
                                    key={course.id}
                                    className="compact-list-item"
                                    style={{
                                        cursor: "pointer",
                                        background: selectedClass === course.id ? "#eff6ff" : "white"
                                    }}
                                    onClick={() => loadClassDetails(course.id)}
                                >
                                    <div>
                                        <strong>{course.title}</strong>
                                        <div className="subtext">{course.semester}</div>
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </div>

                <div className="admin-section">
                    {!classDetails ? (
                        <p>Select a class to manage it.</p>
                    ) : (
                        <>
                            <h3>{classDetails.class.title}</h3>
                            <p>{classDetails.class.description}</p>
                            <p className="subtext">{classDetails.class.semester}</p>

                            <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "20px", marginTop: "20px" }}>
                                <div>
                                    <h4>Students in Class</h4>

                                    <div className="form-group">
                                        <select
                                            multiple
                                            value={selectedStudents}
                                            onChange={(e) =>
                                                setSelectedStudents(
                                                    Array.from(e.target.selectedOptions, (option) => option.value)
                                                )
                                            }
                                            style={{ minHeight: "140px" }}
                                        >
                                            {availableStudents.map((student) => (
                                                <option key={student.id} value={student.id}>
                                                    {student.fullName} ({student.email})
                                                </option>
                                            ))}
                                        </select>
                                    </div>

                                    <button type="button" onClick={handleAddStudents}>
                                        Add Selected Students
                                    </button>

                                    <div style={{ marginTop: "16px" }}>
                                        {classDetails.students.length === 0 ? (
                                            <p>No students enrolled.</p>
                                        ) : (
                                            <div className="compact-list">
                                                {classDetails.students.map((student) => (
                                                    <div key={student.studentId} className="compact-list-item">
                                                        <div>
                                                            <strong>{student.fullName}</strong>
                                                            <div className="subtext">{student.email}</div>
                                                        </div>
                                                        <button
                                                            className="danger-btn"
                                                            onClick={() => handleRemoveStudent(student.studentId)}
                                                        >
                                                            Remove
                                                        </button>
                                                    </div>
                                                ))}
                                            </div>
                                        )}
                                    </div>
                                </div>

                                <div>
                                    <h4>Create Assignment / Quiz / Project</h4>

                                    <form onSubmit={handleCreateContent}>
                                        <div className="form-group">
                                            <select
                                                value={contentForm.type}
                                                onChange={(e) => setContentForm({ ...contentForm, type: e.target.value })}
                                            >
                                                <option value="Homework">Homework</option>
                                                <option value="Quiz">Quiz</option>
                                                <option value="Project">Project</option>
                                            </select>
                                        </div>

                                        <div className="form-group">
                                            <input
                                                placeholder="Title"
                                                value={contentForm.title}
                                                onChange={(e) => setContentForm({ ...contentForm, title: e.target.value })}
                                            />
                                            {contentErrors.title && <p className="field-error">{contentErrors.title}</p>}
                                        </div>

                                        <div className="form-group">
                                            <textarea
                                                placeholder="Description"
                                                value={contentForm.description}
                                                onChange={(e) => setContentForm({ ...contentForm, description: e.target.value })}
                                                rows="4"
                                                style={{ width: "100%", padding: "12px", borderRadius: "8px", border: "1px solid #d1d5db" }}
                                            />
                                            {contentErrors.description && <p className="field-error">{contentErrors.description}</p>}
                                        </div>

                                        <div className="form-group">
                                            <input
                                                type="datetime-local"
                                                value={contentForm.dueDate}
                                                onChange={(e) => setContentForm({ ...contentForm, dueDate: e.target.value })}
                                            />
                                            {contentErrors.dueDate && <p className="field-error">{contentErrors.dueDate}</p>}
                                        </div>

                                        <div className="form-group">
                                            <input
                                                type="file"
                                                onChange={(e) => setContentForm({ ...contentForm, file: e.target.files[0] })}
                                            />
                                        </div>

                                        <button type="submit">Create Content</button>
                                    </form>

                                    <h4 style={{ marginTop: "24px" }}>Create Lecture</h4>

                                    <form onSubmit={handleCreateLecture}>
                                        <div className="form-group">
                                            <input
                                                placeholder="Lecture Title"
                                                value={lectureForm.title}
                                                onChange={(e) => setLectureForm({ ...lectureForm, title: e.target.value })}
                                            />
                                            {lectureErrors.title && <p className="field-error">{lectureErrors.title}</p>}
                                        </div>

                                        <div className="form-group">
                                            <input
                                                type="file"
                                                onChange={(e) => setLectureForm({ ...lectureForm, file: e.target.files[0] })}
                                            />
                                        </div>

                                        <button type="submit">Create Lecture</button>
                                    </form>
                                </div>
                            </div>

                            <h4 style={{ marginTop: "24px" }}>Active Content</h4>
                            {classDetails.activeContent.length === 0 ? (
                                <p>No active items.</p>
                            ) : (
                                <div className="compact-list">
                                    {classDetails.activeContent.map((item) => (
                                        <div key={item.id} className="compact-list-item">
                                            <div>
                                                <strong>[{item.type}] {item.title}</strong>
                                                <div className="subtext">{item.description}</div>
                                                <div className="subtext">Due: {new Date(item.dueDate).toLocaleString()}</div>
                                            </div>

                                            <button
                                                className="danger-btn"
                                                onClick={() => handleDeleteContent(item.id)}
                                            >
                                                Remove
                                            </button>
                                        </div>
                                    ))}
                                </div>
                            )}

                            <h4 style={{ marginTop: "24px" }}>Previous Content</h4>
                            {classDetails.previousContent.length === 0 ? (
                                <p>No previous items.</p>
                            ) : (
                                <div className="compact-list">
                                    {classDetails.previousContent.map((item) => (
                                        <div key={item.id} className="compact-list-item">
                                            <div>
                                                <strong>[{item.type}] {item.title}</strong>
                                                <div className="subtext">{item.description}</div>
                                                <div className="subtext">Due: {new Date(item.dueDate).toLocaleString()}</div>
                                            </div>

                                            <button
                                                className="danger-btn"
                                                onClick={() => handleDeleteContent(item.id)}
                                            >
                                                Remove
                                            </button>
                                        </div>
                                    ))}
                                </div>
                            )}

                            <h4 style={{ marginTop: "24px" }}>Lectures</h4>
                            {classDetails.lectures.length === 0 ? (
                                <p>No lectures yet.</p>
                            ) : (
                                <div className="compact-list">
                                    {classDetails.lectures.map((lecture) => (
                                        <div key={lecture.id} className="compact-list-item">
                                            <div>
                                                <strong>{lecture.title}</strong>
                                                {lecture.filePath && (
                                                    <div className="subtext">{lecture.filePath}</div>
                                                )}
                                            </div>

                                            <button
                                                className="danger-btn"
                                                onClick={() => handleDeleteLecture(lecture.id)}
                                            >
                                                Remove
                                            </button>
                                        </div>
                                    ))}
                                </div>
                            )}
                        </>
                    )}
                </div>
            </div>
        </div>
    );
}