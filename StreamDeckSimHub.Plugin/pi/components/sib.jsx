﻿'use strict';

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
    Menu,
    MenuItem,
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
 * SVG icon "Add Circle Outline"
 */
const AddCircleOutline = () => {
    // see https://unpkg.com/browse/@mui/icons-material@5.8.4/AddCircleOutline.js
    return (
        <SvgIcon>
            <path
                d='M13 7h-2v4H7v2h4v4h2v-4h4v-2h-4V7zm-1-5C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm0 18c-4.41 0-8-3.59-8-8s3.59-8 8-8 8 3.59 8 8-3.59 8-8 8z'/>
        </SvgIcon>
    );
}

/**
 * Renders the container for a single item. If the item is of type "EffectsContainerBase", an action button will be rendered which
 * opens a context menu with the available properties "Gain" and "IsMuted".
 *
 * The content of the item container has to be supplied as "children" elements.
 */
const Item = (props) => {
    const {item} = props;
    const {sourceId, prefix} = React.useContext(Context);
    const [menuAnchor, setMenuAnchor] = React.useState(null);
    const menuOpen = Boolean(menuAnchor);

    const showMenu = (event) => {
        setMenuAnchor(event.currentTarget);
    }

    const closeMenu = () => {
        setMenuAnchor(null);
    }

    const selectedMenuEntry = (property) => {
        closeMenu();
        window.opener.postMessage({
            message: 'shakeItSelected',
            sourceId: sourceId,
            prefix: prefix,
            itemId: item.id,
            itemName: item.name,
            property: property
        }, '*');
        window.close();
    }

    return (
        <ListItem disablePadding={true} secondaryAction={
            item.type === 'EffectsContainerBase' ?
                <React.Fragment>
                    <IconButton onClick={showMenu}>
                        <AddCircleOutline/>
                    </IconButton>
                    <Menu anchorEl={menuAnchor} open={menuOpen} onClose={closeMenu}>
                        <MenuItem onClick={() => selectedMenuEntry('Gain')} selected={item.selectedName === 'gain'}>Gain</MenuItem>
                        <MenuItem onClick={() => selectedMenuEntry('IsMuted')} selected={item.selectedName === 'ismuted'}>IsMuted</MenuItem>
                    </Menu>
                </React.Fragment>
                : ''}
        >
            {props.children}
        </ListItem>
    );
}

/**
 * Leaf item in the list, e.g. an item without any children.
 * As there is no "expand" icon (see "TreeItem"), we render a placeholder, so that everything aligns well.
 */
const LeafItem = ({depth, item}) => {
    return (
        <Item item={item}>
            <ListItemButton sx={{pl: depth * 2}} selected={item.selected}>
                <Box sx={{width: '1em', fontSize: '1.178rem'}}/>
                <ListItemText primary={item.name}/>
            </ListItemButton>
        </Item>
    );
}


/**
 * Tree item in the list, e.g. an item with children. The children will be rendered as collapsible nested list.
 */
const TreeItem = ({depth, item}) => {
    const [open, setOpen] = React.useState(item.expanded);

    const handleClick = () => {
        const newState = !open;
        setOpen(newState);
        // Persist the state also in the model. Otherwise it would fall back on each unmount/mount to the original "item.expanded" value.
        item.expanded = newState;
    }

    return (
        <React.Fragment>
            <Item item={item}>
                <ListItemButton onClick={handleClick} sx={{pl: depth * 2}} selected={item.selected}>
                    {open ? <ExpandLess/> : <ExpandMore/>}
                    <ListItemText primary={item.name}/>
                </ListItemButton>
            </Item>
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

const Context = React.createContext({sourceId: '', prefix: ''});

const ShakeItProfiles = ({profiles}) => {
    return (
        <List>
            {profiles.map((profile, key) => <ListItemFactory key={key} depth={0} item={profile}/>)}
        </List>
    );
}

const NoProfiles = () => {
    return (
        <Box sx={{p: 4}}>
            <h1>No profiles</h1>

            <p>No profiles found in SimHub or connection to SimHub could not be established.</p>
        </Box>
    );
}

/**
 * Displays ShakeIt profiles.
 *
 * @param props.profiles: An array with the ShakeIt profiles.
 * @param props.sourceId: This attribute will be sent back with the event, when an entry was selected.
 * @param props.prefix: This attribute will be sent back with the event, when an entry was selected.
 */
const App = (props) => {
    const [profiles, setProfiles] = React.useState(props.profiles);
    const [sourceId, setSourceId] = React.useState(props.sourceId);
    const [prefix, setPrefix] = React.useState(props.prefix);

    return (
        <ThemeProvider theme={theme}>
            <CssBaseline/>
            {!profiles || profiles.length === 0 ? <NoProfiles/> :
                <Context.Provider value={{sourceId: sourceId, prefix: prefix}}>
                    <ShakeItProfiles profiles={profiles}/>
                </Context.Provider>}
        </ThemeProvider>
    );
}

const root = ReactDOM.createRoot(document.getElementById('root'));
root.render(
    <App profiles={window.profiles ? window.profiles : []}
         sourceId={window.sourceId ? window.sourceId : 'testSourceId'}
         prefix={window.prefix ? window.prefix : 'sib'}/>
);
