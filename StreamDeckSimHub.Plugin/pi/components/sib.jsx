'use strict';

const {
    Box,
    Collapse,
    CssBaseline,
    createTheme,
    IconButton,
    List,
    ListItem,
    ListItemButton,
    ListItemIcon,
    ListItemText,
    SvgIcon,
    ThemeProvider,
    Tooltip
} = MaterialUI;


/**
 * Adjusts the MUI theme to look somewhat like Stream Deck.
 */
const theme = createTheme({
    palette: {
        mode: 'dark',
        background: {
            default: '#2D2D2D'
        }
    },
    typography: {
        fontSize: 11,
    }
});

/**
 * SVG icon "arrow down".
 */
const ExpandMore = () => {
    // see https://unpkg.com/browse/@mui/icons-material@5.8.4/ExpandMore.js
    return (
        <SvgIcon>
            <path d='M16.59 8.59 12 13.17 7.41 8.59 6 10l6 6 6-6z'/>
        </SvgIcon>
    );
}

/**
 * SVG icon "arrpw up"
 */
const ExpandLess = () => {
    // see https://unpkg.com/browse/@mui/icons-material@5.8.4/ExpandLess.js
    return (
        <SvgIcon>
            <path d='m12 8-6 6 1.41 1.41L12 10.83l4.59 4.58L18 14z'/>
        </SvgIcon>
    )
}

/**
 * SVG icon "Copy"
 */
const ContentCopy = () => {
    // see https://unpkg.com/browse/@mui/icons-material@5.8.4/ContentCopy.js
    return (
        <SvgIcon>
            <path
                d='M16 1H4c-1.1 0-2 .9-2 2v14h2V3h12V1zm3 4H8c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h11c1.1 0 2-.9 2-2V7c0-1.1-.9-2-2-2zm0 16H8V7h11v14z'/>
        </SvgIcon>
    );
}

const Item = ({depth, item}) => {
    return (
        <React.Fragment>
            <ListItemText primary={item.name} sx={{pl: depth * 2}}/>
            {item.type === 'EffectsContainerBase' ?
                <Tooltip title='Copy guid to clipboard'>
                    <IconButton onClick={(e) => {
                        e.preventDefault();
                    }}>
                        <ContentCopy/>
                    </IconButton>
                </Tooltip> : ''}
        </React.Fragment>
    );
}

/**
 * Leaf item in the list, e.g. an item without any children.
 * As there is no "expand" icon at the end (see "TreeItem"), we render a placeholder, so that everything aligns well.
 */
const LeafItem = ({depth, item}) => {
    return (
        <ListItemButton>
            <Item depth={depth} item={item}/>
            <Box sx={{width: '1em', fontSize: '1.178rem'}}/>
        </ListItemButton>
    )
}

/**
 * Tree item in the list, e.g. an item with children. The children will be rendered as collapsible nested list.
 */
const TreeItem = ({depth, item}) => {
    const [open, setOpen] = React.useState(false);

    const handleClick = () => {
        setOpen((prev) => !prev);
    }

    return (
        <React.Fragment>
            <ListItemButton onClick={handleClick}>
                <Item depth={depth} item={item}/>
                {open ? <ExpandLess/> : <ExpandMore/>}
            </ListItemButton>
            <Collapse in={open} timeout='auto' unmountOnExit>
                <List component='div'>
                    {item.effectsContainers.map((item, key) => (
                        <ListItemFactory key={key} depth={depth + 1} item={item}/>
                    ))}
                </List>
            </Collapse>
        </React.Fragment>
    );
}

/**
 * Returns a list item element for the given item.
 */
const ListItemFactory = ({depth, item}) => {
    const hasChildren = item.effectsContainers ? item.effectsContainers.length > 0 : false;
    const Component = hasChildren ? TreeItem : LeafItem;
    return (<Component depth={depth} item={item}/>);
}

const testData = [
    {name: 'Profile 1', effectsContainers: [{name: 'RPM', type: 'EffectsContainerBase'}, {name: 'v2'}]},
    {name: 'Profile 2', effectsContainers: [{name: 'Long name for submenu'}, {name: 'Another name'}, {name: 'Gear effects'}]},
    {name: 'Profile 3', effectsContainers: [{name: 'Street effects'}, {name: 'Another name'}, {name: 'Gear effects'}]},
    {
        id: '1',
        name: 'Profile 4',
        effectsContainers: [{
            id: '2',
            name: 'Group 1',
            type: 'EffectsContainerBase',
            effectsContainers: [
                {id: '3', name: 'Deep 1', type: 'EffectsContainerBase'},
                {id: '4', name: 'Deep 2', type: 'EffectsContainerBase'}
            ]
        }]
    }
];

const App = (props) => {
    const [profiles, setProfiles] = React.useState(props.profiles);

    return (
        <ThemeProvider theme={theme}>
            <CssBaseline/>
            <List>
                {profiles.map((profile, key) => <ListItemFactory key={key} depth={0} item={profile}/>)}
            </List>
        </ThemeProvider>
    );
}

const root = ReactDOM.createRoot(document.getElementById('root'));
root.render(
    <App profiles={window.profiles ? window.profiles : testData}/>
);
