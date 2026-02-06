export default function CalendarView({ events, onSelect }) {

    const days = Array.from({ length: 30 }, (_, i) => i + 1);

    const getEvents = (day) =>
        events.filter(e =>
            new Date(e.start).getDate() === day);

    return (
        <div className="grid">
            {days.map(d => (
                <div className="day" key={d}>
                    <b>{d}</b>

                    {getEvents(d).map(e => (
                        <div
                            className="event"
                            onClick={() => onSelect(e)}>
                            {e.title}
                        </div>
                    ))}

                </div>
            ))}
        </div>
    );
}
