import React from "react";
import { GetPeople } from "../data/People";
import TextField from "@material-ui/core/TextField";
import Autocomplete from "@material-ui/lab/Autocomplete";
import CircularProgress from "@material-ui/core/CircularProgress";

let typingTimer = {};

export default function Asynchronous(props) {
  const [open, setOpen] = React.useState(false);
  const [options, setOptions] = React.useState([]);
  const [value, setValue] = React.useState("");
  const [searchValue, setSearchValue] = React.useState("");
  const [loading, setLoading] = React.useState(false);

  React.useEffect(
    () => {
      setLoading(true);
      GetPeople(1, searchValue).then((response) => {
        setOptions(response.data.data);
        setLoading(false);
      });
    },
    [searchValue]
  );

  const doneTyping = () => {
    setSearchValue(value);
  };

  const onChange = event => {
    setValue(event.target.value);
    clearTimeout(typingTimer);
    typingTimer = setTimeout(doneTyping, 900);
  };

  return (
    <Autocomplete
      id="people-autocomplete"
      open={open}
      onOpen={() => {
        setOpen(true);
      }}
      onClose={() => {
        setOpen(false);
      }}
      getOptionSelected={(option, value) => option.name === value.name}
      getOptionLabel={(option) => option.name}
      options={options}
      loading={loading}
      defaultValue={props.defaultValue}
      onChange={(event, newValue) => {
          if(props.handleOnChange){
            props.handleOnChange(newValue);
          }
      }}
      renderInput={(params) => (
        <TextField
          {...params}
          value={value}
          onChange={onChange}
          label="Pessoa"
          variant="outlined"
          fullWidth
          InputProps={{
            ...params.InputProps,
            endAdornment: (
              <React.Fragment>
                {loading ? (
                  <CircularProgress color="inherit" size={20} />
                ) : null}
                {params.InputProps.endAdornment}
              </React.Fragment>
            ),
          }}
        />
      )}
    />
  );
}
